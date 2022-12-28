using System;
using System.Net;

namespace Core.Network;

[Serializable]
public class Version
{
    public Version(int height)
    {
        Height = height;
    }
    
    public int Height { get; }
}