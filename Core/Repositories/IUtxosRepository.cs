﻿using System;
using System.Collections.Generic;

namespace Core.Repositories;

public interface IUtxosRepository
{
    IEnumerable<Utxo> FindUtxosLockedWith(string publicKeyHash);
    
    void DeleteOneIfExists(string transactionHash, int outputIndex);

    void InsertBulk(IEnumerable<Utxo> utxos);
    
    void DeleteAll();
}