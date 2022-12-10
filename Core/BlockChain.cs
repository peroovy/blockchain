using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Core.Transactions;
using Core.Utils;

namespace Core;

public class BlockChain
{
    private readonly List<Block> blocks = new();

    private const int StartSubsidy = 100;
    private const int Difficult = 3;

    public BlockChain()
    {
        var genesis = MineBlock(Hashing.ZeroHash,
            ImmutableArray.Create(Transaction.CreateCoinbase("Anybody", StartSubsidy)), Difficult);
        blocks.Add(genesis);
    }

    public IEnumerable<Block> Blocks => blocks.AsReadOnly();

    public void AddBlock(string address, int amount)
    {
        var previousHash = blocks.Last().Hash;
        var transactions = ImmutableArray.Create(Transaction.CreateCoinbase(address, amount));

        var block = MineBlock(previousHash, transactions, Difficult);

        blocks.Add(block);
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
