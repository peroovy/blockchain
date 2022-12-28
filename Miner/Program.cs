using System;
using System.Configuration;
using System.Net;
using System.Threading;
using Core;
using Microsoft.Extensions.Logging;

namespace Miner;

internal static class Program
{
    private static readonly IPEndPoint Address = new(
        IPAddress.Parse(ConfigurationManager.AppSettings["Address"]),
        Convert.ToInt32(ConfigurationManager.AppSettings["Port"])
    );
    
    private static readonly IPEndPoint Dns = new(
        IPAddress.Parse(ConfigurationManager.AppSettings["DnsAddress"]),
        Convert.ToInt32(ConfigurationManager.AppSettings["DnsPort"])
    );

    private static readonly Logger<MinerNode> Logger = new(LoggerFactory.Create(builder => builder.AddConsole()));

    private const string PrivateKeyPath = "wallet";
    private const string PublicKeyPath = "wallet.pub";
    
    public static void Main(string[] args)
    {
        var wallet = Wallet.Load(PrivateKeyPath, PublicKeyPath);
        var node = new MinerNode(Address, Dns, Logger, wallet);
        node.Run();
        
        Logger.LogInformation($"Listen {Address}");
        
        Thread.Sleep(Timeout.Infinite);
    }
}