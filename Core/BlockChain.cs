using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core.Repositories;
using Core.Transactions;
using Core.Utils;

namespace Core;

public class BlockChain : IEnumerable<Block>
{
    private readonly IBlocksRepository blocksRepository;
    
    private const int StartSubsidy = 100;
    private const int Difficult = 3;

    public BlockChain(IBlocksRepository blocksRepository, string address)
    {
        this.blocksRepository = blocksRepository;
        
        if (blocksRepository.ExistsAny())
            return;
        
        var genesis = MineBlock(Hashing.ZeroHash,
            ImmutableArray.Create(Transaction.CreateCoinbase(address, StartSubsidy)), Difficult);

        blocksRepository.Add(genesis);
    }

    public void AddBlock(ImmutableArray<Transaction> transactions)
    {
        var block = MineBlock(blocksRepository.Last().Hash, transactions, Difficult);

        blocksRepository.Add(block);
    }

    public int GetBalance(string address)
    {
        return FindAllUtxo(address)
            .Select(utxo => utxo.Value)
            .Sum();
    }

    public IEnumerator<Block> GetEnumerator() => blocksRepository.GetAll().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private ImmutableArray<Output> FindAllUtxo(string address)
    {
        var unspentOutputs = ImmutableArray.CreateBuilder<Output>();
        var spentOutputs = new Dictionary<string, HashSet<int>>();

        foreach (var block in blocksRepository.GetAll())
        {
            foreach (var transaction in block.Transactions)
            {
                if (!spentOutputs.TryGetValue(transaction.Hash, out var spentIndices))
                    spentIndices = spentOutputs[transaction.Hash] = new HashSet<int>();

                var utxos = transaction.Outputs
                    .Where((output, i) => !spentIndices.Contains(i) && output.BelongTo(address));
                
                unspentOutputs.AddRange(utxos);
                
                if (transaction.IsCoinbase)
                    break;
                
                foreach (var input in transaction.Inputs.Where(input => input.BelongTo(address)))
                    spentIndices.Add(input.OutputIndex);
            }
        }

        return unspentOutputs.ToImmutable();
    }

    private static Block MineBlock(string previousHash, ImmutableArray<Transaction> transactions, int difficult)
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
}
