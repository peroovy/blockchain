using System;
using System.Net;

namespace Core.Network;

[Serializable]
public class Package
{
    public Package(IPEndPoint addressFrom, PackageTypes packageTypes, byte[] body)
    {
        AddressFrom = addressFrom;
        PackageTypes = packageTypes;
        Body = body;
    }

    public IPEndPoint AddressFrom { get; }
    
    public PackageTypes PackageTypes { get; }
    
    public byte[] Body { get; }
}