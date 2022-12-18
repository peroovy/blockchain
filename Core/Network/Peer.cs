using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Core.Repositories;
using Core.Utils;

namespace Core.Network;

public abstract class Peer : Node
{
    protected readonly Wallet Wallet;
    protected readonly BlockChain BlockChain;
    protected readonly ConcurrentBag<IPEndPoint> Addresses = new();
    
    private readonly IPEndPoint dns;
    private readonly IBlocksRepository blocksRepository;
    private readonly IUtxosRepository utxosRepository;

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

    public void SendPackageToDns()
    {
        var package = new Package(AddressFrom, PackageTypes.Addresses, Array.Empty<byte>());
        
        Send(dns, package);
    }
    
    protected override void HandlePackage(Package package)
    {
        switch (package.PackageTypes)
        {
            case PackageTypes.Addresses:
                StoreNodesFromDns(package);
                HandshakeWithNetwork();
                break;
            
            case PackageTypes.HandshakeWithNetwork:
                StoreNewNode(package);
                SendBlockChainToNewNode(package);
                break;
            
            case PackageTypes.BlockChain:
                UpdateBlockChain(package);
                break;

            case PackageTypes.WantedBlockChain:
                ReceiveWantedBlockChain(package);
                break;
            
            case PackageTypes.Block:
                StoreNewBlock(package);
                break;
        }
    }

    private void StoreNodesFromDns(Package package)
    {
        foreach (var address in Serializer.FromBytes<IPEndPoint[]>(package.Body))
            Addresses.Add(address);
    }

    private void HandshakeWithNetwork()
    {
        if (Addresses.Count == 0 && !blocksRepository.ExistsAny())
        {
            BlockChain.CreateGenesis(Wallet);
            return;    
        }
        
        var height = blocksRepository.GetMaxHeight();
        var version = new Version(height, Wallet.PublicKeyHash);
        
        var handshakePackage = new Package(AddressFrom, PackageTypes.HandshakeWithNetwork, Serializer.ToBytes(version));
        foreach (var address in Addresses)
            Send(address, handshakePackage);
    }

    private void StoreNewNode(Package package)
    {
        Addresses.Add(package.AddressFrom);
    }

    private void SendBlockChainToNewNode(Package package)
    {
        var remoteVersion = Serializer.FromBytes<Version>(package.Body);
        var height = blocksRepository.GetMaxHeight();
        
        if (height == remoteVersion.Height)
            return;

        if (height < remoteVersion.Height)
        {
            var version = new Version(height, Wallet.PublicKeyHash);
            var blockChainPackage = new Package(AddressFrom, PackageTypes.WantedBlockChain, Serializer.ToBytes(version));
            Send(package.AddressFrom, blockChainPackage);
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
        var blockChain = Serializer.FromBytes<SerializedBlockChain>(package.Body);
        
        blocksRepository.DeleteAll();
        blocksRepository.InsertBulk(blockChain.Blocks);

        utxosRepository.DeleteAll();
        utxosRepository.InsertBulk(blockChain.Utxos);
    }

    private void ReceiveWantedBlockChain(Package package)
    {
        var versionFrom = Serializer.FromBytes<Version>(package.Body);
        SendBlockChain(versionFrom, package);
    }

    private void StoreNewBlock(Package package)
    {
        var serializedBlock = Serializer.FromBytes<SerializedBlock>(package.Body);
        var (block, utxos) = (serializedBlock.Block, serializedBlock.Utxos);
        
        var lastBlock = blocksRepository.GetLast();
        var expectedMerkleRoot = MerkleTree
            .Create(serializedBlock.Transactions.Select(tx => tx.Hash))
            .Hash;
        
        if (block.PreviousBlockHash != lastBlock.Hash || block.MerkleRoot != expectedMerkleRoot)
            return;
        
        blocksRepository.Insert(block);
        utxosRepository.InsertBulk(utxos);

        foreach (var spentUtxo in serializedBlock.SpentUtxos)
            utxosRepository.DeleteOneIfExists(spentUtxo.TransactionHash, spentUtxo.Index);
    }
}