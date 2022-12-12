using Core.Utils;

namespace Core.Transactions;

public class Input
{
    public Input(string previousTransactionHash, int outputIndex, string scriptSignature)
    {
        PreviousTransactionHash = previousTransactionHash;
        OutputIndex = outputIndex;
        ScriptSignature = scriptSignature;
        Hash = Hashing
            .SumSHA256(PreviousTransactionHash, OutputIndex.ToString(), ScriptSignature)
            .ToHexDigest();
    }
    
    public string PreviousTransactionHash { get; }
    
    public string Hash { get; }
    
    public int OutputIndex { get; }
    
    public string ScriptSignature { get; }

    public bool BelongTo(string data) => ScriptSignature == data;
}