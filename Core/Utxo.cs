using Core.Transactions;

namespace Core;

public class Utxo
{
    public Utxo(string transactionHash, int outputIndex, Output output)
    {
        TransactionHash = transactionHash;
        OutputIndex = outputIndex;
        Output = output;
    }

    public string TransactionHash { get; }
    
    public int OutputIndex { get; }
    
    public Output Output { get; }
}