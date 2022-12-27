using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Core.Repositories;
using Core.Utils;

namespace Core.Network;

public abstract class Peer : P2PNode
{
    protected readonly Wallet Wallet;
    protected readonly BlockChain BlockChain;
    private readonly ConcurrentDictionary<IPEndPoint, bool> addresses = new();
    
    private readonly IPEndPoint dns;
    private readonly IBlocksRepository blocksRepository;
    private readonly IUtxosRepository utxosRepository;
    
    protected const int Subsidy = 60;
    protected const int Difficult = 4;
    private const int DnsRequestPeriodMilliseconds = 3 * 60 * 1000;

    protected Peer(IPEndPoint address, IPEndPoint dns, 
        Wallet wallet, IBlocksRepository blocksRepository, IUtxosRepository utxosRepository) 
        : base(address.Address, address.Port)
    {
        this.dns = dns;
        Wallet = wallet;
        this.blocksRepository = blocksRepository;
        this.utxosRepository = utxosRepository;
        BlockChain = new BlockChain(blocksRepository, utxosRepository);
    }

    public override void Run()
    {
        base.Run();

        var scheduledGettingActiveNodesThread = new Thread(SendPackageToDns);
        scheduledGettingActiveNodesThread.Start();
    }

    protected override void HandlePackage(Package package)
    {
        switch (package.PackageType)
        {
            case PackageTypes.Addresses:
                UpdateAddresses(package);
                SendVersion();
                break;
            
            case PackageTypes.Version:
                SendBlockChain(package);
                break;
            
            case PackageTypes.BlockChain:
                UpdateBlockChain(package);
                break;

            case PackageTypes.Block:
                AddBlockToChain(package);
                break;
        }
    }
    
    protected void SendBroadcast(Package package)
    {
        foreach (var address in addresses.Keys)
            Task.Run(() => Send(address, package));
    }

    private void UpdateAddresses(Package package)
    {
        foreach (var address in Serializer.FromBytes<IPEndPoint[]>(package.Data))
            addresses.Add(address);
    }

    private void SendVersion()
    {
        if (addresses.Count == 0 && !blocksRepository.ExistsAny())
            BlockChain.CreateGenesis(Wallet);
        
        var height = blocksRepository.GetMaxHeight();
        var version = new Version(height, Wallet.PublicKeyHash);
        
        var versionPackage = new Package(AddressFrom, PackageTypes.Version, Serializer.ToBytes(version));
        SendBroadcast(versionPackage);
    }

    private void SendBlockChain(Package package)
    {
        addresses.Add(package.AddressFrom);

        var remoteVersion = Serializer.FromBytes<Version>(package.Data);
        var height = blocksRepository.GetMaxHeight();
        
        if (height == remoteVersion.Height)
            return;

        if (height < remoteVersion.Height)
        {
            var version = new Version(height, Wallet.PublicKeyHash);
            var versionPackage = new Package(AddressFrom, PackageTypes.Version, Serializer.ToBytes(version));
            Send(package.AddressFrom, versionPackage);
            return;
        }

        SendBlockChain(remoteVersion, package);
    }

    private void SendBlockChain(Version versionFrom, Package package)
    {
        var blocks = blocksRepository
            .GetBlockChain()
            .ToArray();
        var utxos = utxosRepository
            .FindUtxosLockedWith(versionFrom.PublicKeyHash)
            .ToArray();

        var blockChain = new SerializedBlockChain(blocks, utxos);
        var requestPackage = new Package(AddressFrom, PackageTypes.BlockChain, Serializer.ToBytes(blockChain));
        Send(package.AddressFrom, requestPackage);
    }

    private void UpdateBlockChain(Package package)
    {
        var blockChain = Serializer.FromBytes<SerializedBlockChain>(package.Data);
        
        blocksRepository.DeleteAll();
        blocksRepository.InsertBulk(blockChain.Blocks);

        utxosRepository.DeleteAll();
        utxosRepository.InsertBulk(blockChain.Utxos);
    }

    private void AddBlockToChain(Package package)
    {
        var serializedBlock = Serializer.FromBytes<SerializedBlock>(package.Data);
        var (block, utxos) = (serializedBlock.Block, serializedBlock.Utxos);
        
        var lastBlock = blocksRepository.GetLast();
        var expectedMerkleRoot = MerkleTree
            .Create(serializedBlock.Transactions.Select(tx => tx.Hash))
            .Hash;
        
        if (!block.Hash.StartsWithBitsNumber(Difficult) 
            || block.PreviousBlockHash != lastBlock.Hash 
            || block.MerkleRoot != expectedMerkleRoot)
        {
            return;
        }
        
        blocksRepository.Insert(block);
        utxosRepository.InsertBulk(utxos);

        foreach (var spentUtxo in serializedBlock.SpentUtxos)
            utxosRepository.DeleteOneIfExists(spentUtxo.TransactionHash, spentUtxo.Index);
    }
    
    private void SendPackageToDns()
    {
        var package = new Package(AddressFrom, PackageTypes.Addresses, Array.Empty<byte>());

        while (true)
        {
            Send(dns, package);
            Thread.Sleep(DnsRequestPeriodMilliseconds);
        }
    }
}