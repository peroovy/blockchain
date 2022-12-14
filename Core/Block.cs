using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Core.Transactions;
using Core.Utils;

namespace Core;

[Serializable]
public class Block
{
    public Block(
        string previousBlockHash, long timestamp, 
        IReadOnlyList<Transaction> transactions, 
        int difficult, long nonce)
    {
        PreviousBlockHash = previousBlockHash;
        Timestamp = timestamp;
        Difficult = difficult;
        Nonce = nonce;
        Transactions = transactions;
        
        var merkleHash = MerkleTree
            .Create(Transactions.Select(transaction => transaction.Hash))
            .Hash;
        
        Hash = Hashing
            .SumSha256(PreviousBlockHash, Timestamp.ToString(), Difficult.ToString(), Nonce.ToString(), merkleHash)
            .ToHexDigest();
    }
    
    public string PreviousBlockHash { get; }
    
    public string Hash { get; }

    public long Timestamp { get; }
    
    public IReadOnlyList<Transaction> Transactions { get; }
    
    public int Difficult { get; }
    
    public long Nonce { get; }
}