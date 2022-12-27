using System;
using Core.Transactions;

namespace Core.Network;

[Serializable]
public class SerializedBlockChain
{
    public SerializedBlockChain(Block[] blocks, Output[] utxos)
    {
        Blocks = blocks;
        Utxos = utxos;
    }
    
    public Block[] Blocks { get; }
    
    public Output[] Utxos { get; }
}