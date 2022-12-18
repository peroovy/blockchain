using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using Core;
using Microsoft.Extensions.Logging;

namespace DNS;

internal class Program
{
    private static readonly Logger<DnsNode> Logger = new(LoggerFactory.Create(builder => builder.AddConsole()));

    public static void Main(string[] args)
    {
        var address = IPAddress.Parse(ConfigurationManager.AppSettings["Address"]);
        var port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);

        var node = new DnsNode(Logger, address, port);
        node.Run();
        Logger.LogInformation($"Listen {address}:{port}");
    }
}