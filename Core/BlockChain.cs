using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Repositories;
using Core.Transactions;
using Core.Utils;

namespace Core;

public class BlockChain : IEnumerable<Block>
{
    private readonly IBlocksRepository blocksRepository;
    
    private const int Subsidy = 100;
    private const int Difficult = 3;

    public BlockChain(IBlocksRepository blocksRepository, string address)
    {
        this.blocksRepository = blocksRepository;
        
        if (blocksRepository.ExistsAny())
            return;

        var genesis = MineBlock(Hashing.ZeroHash,
            new[] { Transaction.CreateCoinbase(address, Subsidy) },
            Difficult);

        blocksRepository.Add(genesis);
    }

    public void AddBlock(IReadOnlyList<Transaction> transactions)
    {
        var block = MineBlock(blocksRepository.Last().Hash, transactions, Difficult);

        blocksRepository.Add(block);
    }

    public int GetBalance(string address)
    {
        return FindUtxos(address)
            .Select(utxo => utxo.Output.Value)
            .Sum();
    }
    
    public Transaction CreateTransaction(string senderAddress, string receiverAddress, int amount)
    {
        var utxos = FindUtxos(senderAddress);

        var inputs = new List<Input>();
        var accumulated = 0;
        foreach (var utxo in utxos.OrderBy(utxo => utxo.Output.Value))
        {
            var input = new Input(utxo.TransactionHash, utxo.OutputIndex, senderAddress);
            inputs.Add(input);
            
            accumulated += utxo.Output.Value;

            if (accumulated > amount)
                break;
        }

        if (accumulated < amount)
            throw new NotEnoughCurrencyException();

        var outputs = new List<Output> { new(amount, receiverAddress) };
        if (accumulated > amount)
            outputs.Add(new Output(accumulated - amount, senderAddress));

        return new Transaction(inputs, outputs);
    }
    
    public IEnumerator<Block> GetEnumerator() => blocksRepository.GetAll().GetEnumerator();

    private IReadOnlyList<Utxo> FindUtxos(string address)
    {
        var unspentOutputs = new List<Utxo>();
        var spentOutputs = new Dictionary<string, HashSet<int>>();

        foreach (var block in blocksRepository
                     .GetAll()
                     .Reverse())
        {
            foreach (var transaction in block.Transactions)
            {
                if (!spentOutputs.TryGetValue(transaction.Hash, out var spentIndices))
                    spentIndices = spentOutputs[transaction.Hash] = new HashSet<int>();

                var utxos = transaction.Outputs
                    .Select((output, i) => new Utxo(transaction.Hash, i, output))
                    .Where(utxo => !spentIndices.Contains(utxo.OutputIndex) && utxo.Output.CanBeUnlockedWith(address));
                
                unspentOutputs.AddRange(utxos);
                
                if (transaction.IsCoinbase)
                    break;

                foreach (var input in transaction.Inputs.Where(input => input.CanUnlockOutputWith(address)))
                {
                    if (!spentOutputs.ContainsKey(input.PreviousTransactionHash))
                        spentOutputs.Add(input.PreviousTransactionHash, new HashSet<int>());
                    
                    spentOutputs[input.PreviousTransactionHash].Add(input.OutputIndex);
                }
            }
        }

        return unspentOutputs.AsReadOnly();
    }

    private static Block MineBlock(string previousHash, IReadOnlyList<Transaction> transactions, int difficult)
    {
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

        for (var nonce = 0L; nonce < long.MaxValue; nonce++)
        {
            var block = new Block(previousHash, timestamp, transactions, difficult, nonce);

            if (ValidateBlock(block, difficult))
                return block;
        }

        throw new InvalidOperationException("Not found nonce");
    }

    private static bool ValidateBlock(Block block, int expectedDifficult)
    {
        return block.Hash
            .ToBits()
            .Take(expectedDifficult)
            .All(bit => !bit);
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
