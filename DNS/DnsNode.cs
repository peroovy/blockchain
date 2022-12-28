using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Core.Network;
using Core.Utils;
using Microsoft.Extensions.Logging;

namespace DNS;

public class DnsNode : P2PNode
{
    private readonly ILogger logger;
    private readonly ConcurrentDictionary<IPEndPoint, bool> addresses = new();

    public DnsNode(ILogger logger, IPAddress address, int port) : base(address, port)
    {
        this.logger = logger;
    }
    
    protected override void HandlePackage(Package package)
    {
        var addressFrom = package.AddressFrom;
        
        logger.LogInformation($"Connects with {addressFrom}");
        
        var endPoints = addresses
            .Where(pair => !pair.Key.Equals(addressFrom))
            .Select(pair => pair.Key)
            .ToArray();
        addresses[addressFrom] = true;

        var responsePackage = new Package(AddressFrom, PackageTypes.Addresses, Serializer.ToBytes(endPoints));
        Send(addressFrom, responsePackage);
    }
}