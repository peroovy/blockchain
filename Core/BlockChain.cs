using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Core;

public class BlockChain
{
    private readonly List<Block> blocks = new();

    public BlockChain()
    {
        var genesis = ComputeBlock(
            "",
            DateTimeOffset.Now.ToUnixTimeSeconds(),
            "Nikita Samkov had found 1 YOX when he went to KFC",
            3
        );
        blocks.Add(genesis);
    }

    public IEnumerable<Block> Blocks => blocks.AsReadOnly();

    public void Add(string data, int difficult)
    {
        var previousHash = blocks.Last().Hash;
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

        var block = ComputeBlock(previousHash, timestamp, data, difficult);

        blocks.Add(block);
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
    private static Block ComputeBlock(string previousHash, long timestamp, string data, int difficult)
    {
        for (var nonce = 0L; nonce < long.MaxValue; nonce++)
        {
            var hash = Hashing.ToSHA256(
                previousHash, timestamp.ToString(), data, difficult.ToString(), nonce.ToString()
            );

            var proof = hash
                .ToBits()
                .Take(difficult)
                .All(bit => !bit);

            if (proof)
                return new Block(previousHash, hash.ToHexDigest(), timestamp, data, difficult, nonce);
        }

        throw new InvalidOperationException("Not found nonce");
    }
}
