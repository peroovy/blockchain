using System;

namespace Core;

[Serializable]
public class SerializedUtxo
{
    public string TransactionHash { get; set; }
    
    public int OutputIndex { get; set; }
    
    public byte[] Output { get; set; }
}