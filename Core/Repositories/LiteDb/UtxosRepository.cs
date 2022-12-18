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

    public IEnumerable<Utxo> FindUtxosLockedWith(string publicKeyHash) =>
        utxosCollection.Find(utxo => utxo.PublicKeyHash == publicKeyHash);
    
    public void DeleteOneIfExists(string transactionHash, int outputIndex)
    {
        utxosCollection.DeleteMany(utxo => utxo.TransactionHash == transactionHash && utxo.Index == outputIndex);
    }

    public void InsertBulk(IEnumerable<Utxo> utxos) => utxosCollection.InsertBulk(utxos);

    public void DeleteAll() => utxosCollection.DeleteAll();
}