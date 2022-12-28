﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Utils;

namespace Core.Network;

public abstract class Peer : P2PNode
{
    protected readonly Wallet Wallet;
    protected readonly BlockChain BlockChain;
    
    private readonly IPEndPoint dns;
    private readonly ConcurrentDictionary<IPEndPoint, bool> addresses = new();

    protected Peer(IPEndPoint address, IPEndPoint dns, Wallet wallet) : base(address.Address, address.Port)
    {
        this.dns = dns;
        Wallet = wallet;
        BlockChain = new BlockChain();
    }

    public override void Run()
    {
        base.Run();

        var package = new Package(AddressFrom, PackageTypes.Connection, Array.Empty<byte>());
        Send(dns, package);
        
        if (BlockChain.IsEmpty)
            BlockChain.CreateGenesis(Wallet);
    }

    protected override void HandlePackage(Package package)
    {
        if (package.PackageType != PackageTypes.Addresses)
            addresses.Add(package.AddressFrom);
        
        switch (package.PackageType)
        {
            case PackageTypes.Addresses:
            {
                foreach (var addr in Serializer.FromBytes<IPEndPoint[]>(package.Data))
                    addresses.Add(addr);
                
                SendVersion();
                break;
            }
            
            case PackageTypes.Version:
                SendBlockChain(package);
                break;
            
            case PackageTypes.BlockChain:
                UpdateBlockChain(package);
                break;

            case PackageTypes.Block:
                AddBlock(package);
                break;
        }
    }
    
    protected void SendBroadcast(Package package)
    {
        foreach (var address in addresses.Keys)
            Task.Run(() => Send(address, package));
    }

    private void SendVersion()
    {
        var version = new Version(BlockChain.Height, Wallet.PublicKeyHash);
        var versionPackage = new Package(AddressFrom, PackageTypes.Version, Serializer.ToBytes(version));
        
        SendBroadcast(versionPackage);
    }

    private void SendBlockChain(Package package)
    {
        var remoteVersion = Serializer.FromBytes<Version>(package.Data);
        var height = BlockChain.Height;
        
        if (height == remoteVersion.Height)
            return;

        if (height < remoteVersion.Height)
        {
            var version = new Version(height, Wallet.PublicKeyHash);
            var versionPackage = new Package(AddressFrom, PackageTypes.Version, Serializer.ToBytes(version));
            Send(package.AddressFrom, versionPackage);
            return;
        }
        
        var data = Serializer.ToBytes(new SerializedBlockChain(BlockChain.ToArray()));
        var blockChainPackage = new Package(AddressFrom, PackageTypes.BlockChain, data);
        Send(package.AddressFrom, blockChainPackage);
    }
    
    private void UpdateBlockChain(Package package)
    {
        var blockChain = Serializer.FromBytes<SerializedBlockChain>(package.Data);

        BlockChain.Reindex(blockChain.Blocks);
    }

    private void AddBlock(Package package)
    {
        var block = Serializer.FromBytes<Block>(package.Data);

        BlockChain.TryAddBlock(block);
    }
}