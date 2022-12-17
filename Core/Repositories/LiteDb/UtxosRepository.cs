using System;
using System.Collections.Generic;
using LiteDB;

namespace Core.Repositories.LiteDB;

public class UtxosRepository : IUtxosRepository
{
    private readonly ILiteCollection<Utxo> utxosCollection;
    
    public UtxosRepository(ILiteDatabase database)
    {
        utxosCollection = database.GetCollection<Utxo>();
    }

    public IEnumerable<Utxo> FindLockedUtxosWith(string publicKeyHash) =>
        utxosCollection.Find(utxo => utxo.PublicKeyHash == publicKeyHash);
    
    public void DeleteOne(string transactionHash, int outputIndex)
    {
        var num = utxosCollection.DeleteMany(
            utxo => utxo.TransactionHash == transactionHash && utxo.Index == outputIndex
        );

        if (num != 1)
        {
            throw new InvalidOperationException(
                $"Not found with transaction hash '{transactionHash}' and index {outputIndex}");
        }
    }

    public void InsertBulk(IEnumerable<Utxo> utxos) => utxosCollection.InsertBulk(utxos);
}