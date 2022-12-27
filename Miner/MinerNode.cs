using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Core;
using Core.Network;
using Core.Transactions;
using Core.Utils;
using Microsoft.Extensions.Logging;

namespace Miner;

public class MinerNode : Peer
{
    private readonly ILogger logger;
    private readonly ConcurrentQueue<Transaction> mempool = new();

    private const int MaxMempoolLength = 1;

    public MinerNode(IPEndPoint address, IPEndPoint dns, ILogger logger, Wallet wallet) : base(address, dns, wallet)
    {
        this.logger = logger;
    }

    protected override void HandlePackage(Package package)
    {
        base.HandlePackage(package);
        
        switch (package.PackageType)
        {
            case PackageTypes.Transaction:
                HandleTransaction(package);
                break;
        }
    }

    private void HandleTransaction(Package package)
    {
        var transaction = Serializer.FromBytes<Transaction>(package.Data);
        
        if (!ValidateNewTransaction(transaction))
            return;
        
        mempool.Enqueue(transaction);
        logger.LogInformation($"Store new transaction({mempool.Count}): {transaction.Hash}");

        var transactions = TakeTransactions();
        if (transactions is null)
            return;

        var block = BlockChain.CreateBlock(Wallet, transactions);
        logger.LogInformation($"Successful mining block {block.Hash}, difficult {block.Difficult}");
        
        var blockPackage = new Package(AddressFrom, PackageTypes.Block, Serializer.ToBytes(block));
        SendBroadcast(blockPackage);
    }

    private Transaction[] TakeTransactions()
    {
        var transactions = mempool
            .Take(MaxMempoolLength)
            .ToArray();
        
        if (transactions.Length < MaxMempoolLength)
            return null;

        for (var i = 0; i < transactions.Length; i++)
            mempool.TryDequeue(out _);

        return transactions;
    }

    private bool ValidateNewTransaction(Transaction transaction)
    {
        var existLockedOutputs = ExistLockedOutputs(transaction);

        return transaction.VerifiedSignature && !transaction.IsSelfToSelf && !existLockedOutputs;
    }

    private bool ExistLockedOutputs(Transaction transaction)
    {
        var uniqueSelectedOutputs = mempool
            .SelectMany(tx => tx.Inputs)
            .Select(input => input.OutputHash)
            .ToHashSet();
        var existedLockedOutputs = transaction
            .Inputs
            .Select(input => input.OutputHash)
            .Any(output => uniqueSelectedOutputs.Contains(output));

        return existedLockedOutputs;
    }
}