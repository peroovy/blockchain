using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Core.Network;
using Core.Utils;
using Microsoft.Extensions.Logging;

namespace DNS;

public class DnsNode : Node
{
    private readonly ILogger logger;
    private static readonly ConcurrentDictionary<IPEndPoint, long> Addresses = new();

    public DnsNode(ILogger logger, IPAddress address, int port) : base(address, port)
    {
        this.logger = logger;
    }
    
    protected override void HandlePackage(Package package)
    {
        var addressFrom = package.AddressFrom;
        logger.LogInformation($"Connect with {addressFrom}");
        
        SendBroadcast(addressFrom);

        var addresses = Addresses
            .Keys
            .Where(address => !address.Equals(addressFrom))
            .ToArray();
        Addresses[addressFrom] = DateTime.Now.Ticks;

        var responsePackage = new Package(AddressFrom, PackageTypes.Addresses, Serializer.ToBytes(addresses));
        Send(addressFrom, responsePackage);
    }
    
    private void SendBroadcast(IPEndPoint addressFrom)
    {
        var package = new Package(AddressFrom, PackageTypes.Broadcast, Array.Empty<byte>());
        
        foreach (var address in Addresses
                     .Keys
                     .Where(point => !point.Equals(addressFrom))
                     .ToArray())
        {
            try
            {
                Send(address, package);
            }
            catch (SocketException)
            {
                if (Addresses.TryRemove(address, out _))
                    logger.LogInformation($"Disconnect node: {address}");
            }
        }
    }
}