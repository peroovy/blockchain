using System.Collections.Concurrent;
using System.Linq;
using System.Net;
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

    public MinerNode(IPEndPoint address, IPEndPoint dns, ILogger logger,
        Wallet wallet, IBlocksRepository blocksRepository, IUtxosRepository utxosRepository) 
        : base(address, dns, wallet, blocksRepository, utxosRepository)
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

        if (mempool.Count < MaxMempoolLength)
            return;

        var (block, utxos) = BlockChain.MineBlock(Wallet, mempool, Subsidy, Difficult);
        logger.LogInformation($"Successful mining block {block.Hash}, difficult {block.Difficult}");
        
        while (mempool.Count > 0)
            mempool.TryDequeue(out _);

        var spentUtxos = block
            .Transactions
            .SelectMany(tx => tx.Inputs)
            .Select(input => new SpentUtxo(input.PreviousTransactionHash, input.OutputIndex))
            .ToArray();
        
        var serializedBlock = new SerializedBlock(block, block.Transactions, utxos, spentUtxos);
        var blockPackage = new Package(AddressFrom, PackageTypes.Block, Serializer.ToBytes(serializedBlock));
        foreach (var address in Addresses.Keys)
            Send(address, blockPackage);
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
            .Select(input => (input.PreviousTransactionHash, input.OutputIndex))
            .ToHashSet();
        var existedLockedOutputs = transaction
            .Inputs
            .Select(input => (input.PreviousTransactionHash, input.OutputIndex))
            .Any(output => uniqueSelectedOutputs.Contains(output));

        return existedLockedOutputs;
    }
}