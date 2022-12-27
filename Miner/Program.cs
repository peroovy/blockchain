using System;
using System.Configuration;
using System.Net;
using System.Threading;
using Core;
using Core.Repositories.LiteDB;
using LiteDB;
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

    private const string BlockChainDb = "blockchain.db";
    private const string PrivateKeyPath = "wallet";
    private const string PublicKeyPath = "wallet.pub";
    
    public static void Main(string[] args)
    {
        using var database = new LiteDatabase(BlockChainDb);
        var blocksRepository = new BlocksRepository(database);
        var utxosRepository = new UtxosRepository(database);
        var wallet = Wallet.Load(PrivateKeyPath, PublicKeyPath);

        var node = new MinerNode(Address, Dns, Logger, wallet, blocksRepository, utxosRepository);
        node.Run();
        
        Logger.LogInformation($"Start on {Address}");
        
        Thread.Sleep(Timeout.Infinite);
    }
}