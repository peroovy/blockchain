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

    public BlockChain(IBlocksRepository blocksRepository, Wallet peer)
    {
        this.blocksRepository = blocksRepository;
        
        if (blocksRepository.ExistsAny())
            return;

        var genesis = MineBlock(Hashing.ZeroHash,
            new[] { Transaction.CreateCoinbase(peer, Subsidy) },
            Difficult);
        
        blocksRepository.Add(genesis);
    }

    public void AddBlock(IReadOnlyList<Transaction> transactions)
    {
        var block = MineBlock(blocksRepository.Last().Hash, transactions, Difficult);

        blocksRepository.Add(block);
    }

    public int GetBalance(string publicKeyHash)
    {
        return FindUtxos(publicKeyHash)
            .Select(utxo => utxo.Output.Value)
            .Sum();
    }
    
    public Transaction CreateTransaction(Wallet sender, string receiverAddress, int amount)
    {
        var utxos = FindUtxos(sender.PublicKeyHash);

        var inputs = new List<Input>();
        var accumulated = 0;
        foreach (var utxo in utxos.OrderBy(utxo => utxo.Output.Value))
        {
            var input = new Input(utxo.TransactionHash, utxo.OutputIndex, sender.PublicKey);
            inputs.Add(input);
            
            accumulated += utxo.Output.Value;

            if (accumulated > amount)
                break;
        }

        if (accumulated < amount)
            throw new NotEnoughCurrencyException();

        var outputs = new List<Output> { new(amount, RsaUtils.GetPublicKeyHashFromAddress(receiverAddress)) };
        if (accumulated > amount)
            outputs.Add(new Output(accumulated - amount, sender.PublicKeyHash));

        var transaction = new Transaction(inputs, outputs);
        var signature = RsaUtils.SignData(sender.PrivateKey, transaction.Hash);

        foreach (var input in inputs)
            input.Signature = signature;

        return transaction;
    }
    
    public IEnumerator<Block> GetEnumerator() => blocksRepository.GetAll().GetEnumerator();

    private IReadOnlyList<Utxo> FindUtxos(string publicKeyHash)
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
                    .Where(utxo => !spentIndices.Contains(utxo.OutputIndex) && utxo.Output.IsLockedFor(publicKeyHash));
                
                unspentOutputs.AddRange(utxos);
                
                if (transaction.IsCoinbase)
                    break;

                foreach (var input in transaction.Inputs.Where(input => input.BelongsTo(publicKeyHash)))
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

        if (transactions.Any(transaction => !transaction.IsCoinbase && !transaction.VerifySignature()))
            throw new InvalidTransactionSignatureException();

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
