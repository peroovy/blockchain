using System;

namespace Core.Network;

[Serializable]
public class SerializedBlockChain
{
    public SerializedBlockChain(Block[] blocks)
    {
        Blocks = blocks;
    }
    
    public Block[] Blocks { get; }
}