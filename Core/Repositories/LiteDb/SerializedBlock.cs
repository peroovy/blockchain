using System;

namespace Core.Repositories;

[Serializable]
public class SerializedBlock
{
    public string Hash { get; set; }
    
    public long Timestamp { get; set; }
    
    public byte[] Data { get; set; }
}