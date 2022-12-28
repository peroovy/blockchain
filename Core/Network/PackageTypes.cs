using System;

namespace Core.Network;

[Serializable]
public enum PackageTypes
{
    Addresses,
    Version,
    BlockChain,
    Transaction,
    Block,
    Connection
}