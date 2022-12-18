using System;

namespace Core.Network;

[Serializable]
public class SpentUtxo
{
    public SpentUtxo(string transactionHash, int index)
    {
        TransactionHash = transactionHash;
        Index = index;
    }
    
    public string TransactionHash { get; }
    
    public int Index { get; }
}