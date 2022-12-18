using System;

namespace Core.Network;

[Serializable]
public class SerializedBlockChain
{
    public SerializedBlockChain(Block[] blocks, Utxo[] utxos)
    {
        Blocks = blocks;
        Utxos = utxos;
    }
    
    public Block[] Blocks { get; }
    
    public Utxo[] Utxos { get; }
}