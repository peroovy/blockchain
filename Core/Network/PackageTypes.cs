using System;

namespace Core.Network;

[Serializable]
public enum PackageTypes
{
    Broadcast,
    Addresses,
    HandshakeWithNetwork,
    BlockChain,
    WantedBlockChain
}