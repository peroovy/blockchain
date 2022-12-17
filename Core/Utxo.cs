using System;
using Core.Transactions;

namespace Core;

[Serializable]
public class Utxo
{
    public Utxo(string transactionHash, int index, int value, string publicKeyHash)
    {
        TransactionHash = transactionHash;
        Index = index;
        Value = value;
        PublicKeyHash = publicKeyHash;
    }

    public string TransactionHash { get; }
    
    public int Index { get; }
    
    public int Value { get; }
    
    public string PublicKeyHash { get; }
}