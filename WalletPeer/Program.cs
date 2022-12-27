using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using Core;
using WalletPeer.Commands;

namespace WalletPeer;

public static class Program
{
    private static readonly IPEndPoint Address = new(
        IPAddress.Parse(ConfigurationManager.AppSettings["Address"]),
        Convert.ToInt32(ConfigurationManager.AppSettings["Port"])
    );
    
    private static readonly IPEndPoint Dns = new(
        IPAddress.Parse(ConfigurationManager.AppSettings["DnsAddress"]),
        Convert.ToInt32(ConfigurationManager.AppSettings["DnsPort"])
    );

    private const string PrivateKeyPath = "wallet";
    private const string PublicKeyPath = "wallet.pub";

    public static void Main(string[] args)
    {
        var wallet = Wallet.Load(PrivateKeyPath, PublicKeyPath);

        var node = new WalletNode(Address, Dns, wallet);
        node.Run();
        
        var commands = GetConsoleCommands(node);
        
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

    private static Dictionary<string, ICommand> GetConsoleCommands(WalletNode node)
    {
        var commands = new List<ICommand>
        {
            new BalanceCommand(node), 
            new SendCommand(node),
            new AddressCommand(node)
        };
        
        var help = new HelpCommand(commands);
        commands.Add(help);

        return commands.ToDictionary(command => command.Name, command => command);
    }
}