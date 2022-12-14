using System;

namespace Core.Repositories;

[Serializable]
public class SerializedUtxo
{
    public string TransactionHash { get; set; }
    
    public int OutputIndex { get; set; }
    
    public byte[] Output { get; set; }
}