using System.Collections.Immutable;
using System.Linq;
using Core.Transactions;
using Core.Utils;

namespace Core;

public class Block
{
    public Block(
        string previousHash, long timestamp, 
        ImmutableArray<Transaction> transactions, 
        int difficult, long nonce)
    {
        PreviousHash = previousHash;
        Timestamp = timestamp;
        Difficult = difficult;
        Nonce = nonce;
        TransactionsTree = MerkleTree.Create(transactions.Select(transaction => transaction.Hash));
        Hash = Hashing
            .SumSHA256(PreviousHash, Timestamp.ToString(), Difficult.ToString(), Nonce.ToString(), TransactionsTree.Hash)
            .ToHexDigest();
    }
    
    public string PreviousHash { get; }
    
    public string Hash { get; }

    public long Timestamp { get; }
    
    public MerkleTree TransactionsTree { get; }
    
    public int Difficult { get; }
    
    public long Nonce { get; }
}