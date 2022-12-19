using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Text;
using Core;
using Core.Network;
using Core.Repositories;
using Core.Transactions;
using Core.Utils;
using Microsoft.Extensions.Logging;

namespace Miner;

public class MinerNode : Peer
{
    private readonly ILogger logger;
    private readonly ConcurrentQueue<Transaction> mempool = new();

    private const int MaxMempoolLength = 1;
    private const int Subsidy = 60;
    private const int Difficult = 5;
    
    public MinerNode(IPEndPoint address, IPEndPoint dns, ILogger logger,
        Wallet wallet, IBlocksRepository blocksRepository, IUtxosRepository utxosRepository) 
        : base(address, dns, wallet, blocksRepository, utxosRepository)
    {
        this.logger = logger;
    }

    protected override void HandlePackage(Package package)
    {
        base.HandlePackage(package);
        
        switch (package.PackageTypes)
        {
            case PackageTypes.Transaction:
                HandleNewTransaction(package);
                break;
        }
    }

    private void HandleNewTransaction(Package package)
    {
        var transaction = Serializer.FromBytes<Transaction>(package.Body);
        
        if (!ValidateNewTransaction(transaction))
            return;
        
        mempool.Enqueue(transaction);
        logger.LogInformation($"Store new transaction({mempool.Count}): {transaction.Hash}");
        Send(package.AddressFrom,
            new Package(AddressFrom, PackageTypes.TransactionConfirmation, Encoding.UTF8.GetBytes(transaction.Hash))
        );
        
        if (mempool.Count < MaxMempoolLength)
            return;

        var (block, transactions, utxos, inputs) = BlockChain.MineBlock(Wallet, mempool, Subsidy, Difficult);
        logger.LogInformation($"Successful mining block {block.Hash}, difficult {block.Difficult}");
        
        while (mempool.Count > 0)
            mempool.TryDequeue(out _);

        var spentUtxos = inputs
            .Select(input => new SpentUtxo(input.PreviousTransactionHash, input.OutputIndex))
            .ToArray();
        var serializedBlock = new SerializedBlock(block, transactions, utxos, spentUtxos);
        var blockPackage = new Package(AddressFrom, PackageTypes.Block, Serializer.ToBytes(serializedBlock));
        foreach (var address in Addresses.Keys)
            Send(address, blockPackage);
    }

    private bool ValidateNewTransaction(Transaction transaction)
    {
        var verifiedSignature = transaction.VerifySignature();

        var uniqueSelectedOutputs = mempool
            .SelectMany(tx => tx.Inputs)
            .Select(input => (input.PreviousTransactionHash, input.OutputIndex))
            .ToHashSet();
        var existedLockedOutputs = transaction
            .Inputs
            .Select(input => (input.PreviousTransactionHash, input.OutputIndex))
            .Any(output => uniqueSelectedOutputs.Contains(output));

        return verifiedSignature && !existedLockedOutputs;
    }
}