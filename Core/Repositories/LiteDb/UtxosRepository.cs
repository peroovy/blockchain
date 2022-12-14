using System;
using System.Collections.Generic;
using System.Linq;
using Core.Transactions;
using Core.Utils;
using LiteDB;

namespace Core.Repositories.LiteDB;

public class UtxosRepository : IUtxosRepository
{
    private readonly ILiteCollection<SerializedUtxo> utxosCollection;
    
    public UtxosRepository(ILiteDatabase database)
    {
        utxosCollection = database.GetCollection<SerializedUtxo>();
    }

    public IEnumerable<Utxo> Filter(Func<Utxo, bool> predicate)
    {
        return utxosCollection
            .FindAll()
            .Select(serialized => new Utxo(serialized.TransactionHash, serialized.OutputIndex,
                Serializer.FromBytes<Output>(serialized.Output)))
            .Where(predicate);
    }

    public void DeleteOne(string transactionHash, int outputIndex)
    {
        var num = utxosCollection.DeleteMany(
            utxo => utxo.TransactionHash == transactionHash && utxo.OutputIndex == outputIndex);

        if (num != 1)
        {
            throw new InvalidOperationException(
                $"Not found with transaction hash '{transactionHash}' and index {outputIndex}");
        }
    }

    public void InsertBulk(IEnumerable<Utxo> utxos)
    {
        var serialized = utxos
            .Select(utxo => new SerializedUtxo
            {
                TransactionHash = utxo.TransactionHash,
                OutputIndex = utxo.OutputIndex,
                Output = Serializer.ToBytes(utxo.Output)
            });
        
        utxosCollection.InsertBulk(serialized);
    }
}