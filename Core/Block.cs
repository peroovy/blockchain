using System;
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
    public Block() {}
    
    [BsonId]
    public int Id { get; set; }
    
    public string PreviousBlockHash { get; set; }
    
    public int Height { get; set; }

    public string Hash { get; set; }

    public long Timestamp { get; set; }
    
    [BsonRef]
    public Transaction[] Transactions { get; set; }
    
    public string MerkleRoot { get; set; }
    
    public int Difficult { get; set; }
    
    public long Nonce { get; set; }
}