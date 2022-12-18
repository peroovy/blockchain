using System;
using System.Net;

namespace Core.Network;

[Serializable]
public class Version
{
    public Version(int height, string publicKeyHash)
    {
        Height = height;
        PublicKeyHash = publicKeyHash;
    }
    
    public int Height { get; }
    
    public string PublicKeyHash { get; }
}