using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Net;
using Core;
using Core.Repositories;
using Core.Repositories.LiteDB;
using LiteDB;
using WalletPeer.Commands;

namespace WalletPeer;

public class Program
{
    public static void Main(string[] args)
    {
        var address = new IPEndPoint(
            IPAddress.Parse(ConfigurationManager.AppSettings["Address"]),
            // Convert.ToInt32(ConfigurationManager.AppSettings["Port"])
            port: Convert.ToInt32(Console.ReadLine())
        );
        var dns = new IPEndPoint(
            IPAddress.Parse(ConfigurationManager.AppSettings["DnsAddress"]),
            Convert.ToInt32(ConfigurationManager.AppSettings["DnsPort"])
        );

        using var database = new LiteDatabase("blockchain.db");
        var blocksRepository = new BlocksRepository(database);
        var utxosRepository = new UtxosRepository(database);
        var wallet = Wallet.LoadFrom("wallet", "wallet.pub");

        var node = new WalletNode(address, dns, wallet, blocksRepository, utxosRepository);
        node.Run();
        node.SendPackageToDns();
        
        var commands = GetConsoleCommands();
        
        while (true)
        {
            Console.Write(">> ");
            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                return;
        
            if (!commands.TryGetValue(input.Trim(), out var command))
            {
                Console.WriteLine("ERROR: Unknown command");
                continue;
            }
            
            command.Execute();
        }
    }

    private static Dictionary<string, ICommand> GetConsoleCommands()
    {
        var commands = new List<ICommand>
        {
            new BalanceCommand(), 
            new SendCommand()
        };
        
        var help = new HelpCommand(commands);
        commands.Add(help);

        return commands.ToDictionary(command => command.Name, command => command);
    }
}