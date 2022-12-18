using System;
using Core.Transactions;

namespace Core.Network;

[Serializable]
public class SerializedBlock
{
    public SerializedBlock(Block block, Transaction[] transactions, Utxo[] utxos, SpentUtxo[] spentUtxos)
    {
        Block = block;
        Transactions = transactions;
        Utxos = utxos;
        SpentUtxos = spentUtxos;
    }
    
    public Block Block { get; }
    
    public Transaction[] Transactions { get; }

    public Utxo[] Utxos { get; }
    
    public SpentUtxo[] SpentUtxos { get; }
}