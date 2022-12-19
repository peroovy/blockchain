using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Core.Network;
using Core.Utils;
using Microsoft.Extensions.Logging;

namespace DNS;

public class DnsNode : Node
{
    private readonly ILogger logger;
    private static readonly ConcurrentDictionary<IPEndPoint, long> Addresses = new();

    private const int TtlSeconds = 3 * 60;

    public DnsNode(ILogger logger, IPAddress address, int port) : base(address, port)
    {
        this.logger = logger;
    }
    
    protected override void HandlePackage(Package package)
    {
        var addressFrom = package.AddressFrom;
        
        logger.LogInformation($"Connect with {addressFrom}");
        
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
        var addresses = Addresses
            .Where(pair => !pair.Key.Equals(addressFrom) && now < pair.Value + TtlSeconds)
            .Select(pair => pair.Key)
            .ToArray();
        Addresses[addressFrom] = now;

        var responsePackage = new Package(AddressFrom, PackageTypes.Addresses, Serializer.ToBytes(addresses));
        Send(addressFrom, responsePackage);
    }
}