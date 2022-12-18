using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Core;
using Core.Network;
using Core.Repositories;
using Core.Transactions;
using Core.Utils;
using Version = Core.Network.Version;

namespace WalletPeer;

public class WalletNode : Node
{
    private readonly IPEndPoint dns;
    private readonly Wallet wallet;
    private readonly IBlocksRepository blocksRepository;
    private readonly IUtxosRepository utxosRepository;
    private readonly BlockChain blockChain;
    private readonly ConcurrentBag<IPEndPoint> addresses = new();

    public WalletNode(IPEndPoint address, IPEndPoint dns, 
        Wallet wallet, IBlocksRepository blocksRepository, IUtxosRepository utxosRepository) 
        : base(address.Address, address.Port)
    {
        this.dns = dns;
        this.wallet = wallet;
        this.blocksRepository = blocksRepository;
        this.utxosRepository = utxosRepository;
        blockChain = new BlockChain(wallet, blocksRepository, utxosRepository);
    }

    public int Balance => blockChain.GetBalance(wallet.PublicKeyHash);

    public void SendPackageToDns()
    {
        var package = new Package(AddressFrom, PackageTypes.Addresses, Array.Empty<byte>());
        
        Send(dns, package);
    }

    public Transaction AddTransactionToMempool(string receiverAddress, int amount)
    {
        var transaction = blockChain.CreateTransaction(wallet, receiverAddress, amount);

        var serializedTransaction = Serializer.ToBytes(transaction);
        var package = new Package(AddressFrom, PackageTypes.Transaction, serializedTransaction);
        foreach (var address in addresses)
            Send(address, package);
        
        return transaction;
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
        }
    }

    private void StoreNodesFromDns(Package package)
    {
        foreach (var address in Serializer.FromBytes<IPEndPoint[]>(package.Body))
            addresses.Add(address);
    }

    private void HandshakeWithNetwork()
    {
        var height = blocksRepository.GetMaxHeight();
        var version = new Version(height, wallet.PublicKeyHash);
        
        var handshakePackage = new Package(AddressFrom, PackageTypes.HandshakeWithNetwork, Serializer.ToBytes(version));
        foreach (var address in addresses)
            Send(address, handshakePackage);
    }

    private void StoreNewNode(Package package)
    {
        addresses.Add(package.AddressFrom);
    }

    private void SendBlockChainToNewNode(Package package)
    {
        var remoteVersion = Serializer.FromBytes<Version>(package.Body);
        var height = blocksRepository.GetMaxHeight();
        
        if (height == remoteVersion.Height)
            return;

        if (height < remoteVersion.Height)
        {
            var version = new Version(height, wallet.PublicKeyHash);
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
}