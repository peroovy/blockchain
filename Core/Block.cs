using System;
using System.Collections.Immutable;
using System.Linq;
using Core.Transactions;
using Core.Utils;
using LiteDB;

namespace Core;

[Serializable]
public class Block
{
    public Block(
        string previousBlockHash, int height, long timestamp, 
        Transaction[] transactions, 
        int difficult, long nonce)
    {
        PreviousBlockHash = previousBlockHash;
        Height = height;
        Timestamp = timestamp;
        Difficult = difficult;
        Nonce = nonce;
        Transactions = transactions;
        
        MerkleRoot = MerkleTree
            .Create(Transactions.Select(transaction => transaction.Hash))
            .Hash;

        Hash = Hashing
            .SumSha256(PreviousBlockHash, Timestamp.ToString(), Difficult.ToString(), Nonce.ToString(), MerkleRoot)
            .ToHexDigest();
    }
    
    [BsonCtor]
    public Block(ObjectId _id,
        string previousBlockHash, int height, string hash, long timestamp, string merkleRoot, int difficult, long nonce)
    {
        PreviousBlockHash = previousBlockHash;
        Height = height;
        Hash = hash;
        Timestamp = timestamp;
        MerkleRoot = merkleRoot;
        Difficult = difficult;
        Nonce = nonce;
    }
    
    public string PreviousBlockHash { get; }
    
    public int Height { get; }

    public string Hash { get; }

    public long Timestamp { get; }
    
    [BsonIgnore]
    [field: NonSerialized]
    public Transaction[] Transactions { get; }
    
    public string MerkleRoot { get; }
    
    public int Difficult { get; }
    
    public long Nonce { get; }
}