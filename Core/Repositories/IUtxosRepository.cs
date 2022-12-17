using System;
using System.Collections.Generic;

namespace Core.Repositories;

public interface IUtxosRepository
{
    IEnumerable<Utxo> FindLockedUtxosWith(string publicKeyHash);
    
    void DeleteOne(string transactionHash, int outputIndex);

    void InsertBulk(IEnumerable<Utxo> utxos);
}