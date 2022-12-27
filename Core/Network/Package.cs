using System;
using System.Net;

namespace Core.Network;

[Serializable]
public class Package
{
    public Package(IPEndPoint addressFrom, PackageTypes packageType, byte[] data)
    {
        AddressFrom = addressFrom;
        PackageType = packageType;
        Data = data;
    }

    public IPEndPoint AddressFrom { get; }
    
    public PackageTypes PackageType { get; }
    
    public byte[] Data { get; }
}