﻿using System;
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

    private const string BlockChainDb = "blockchain.db";
    private const string PrivateKeyPath = "wallet";
    private const string PublicKeyPath = "wallet.pub";

    public static void Main(string[] args)
    {
        var confirmedTransactions = new Queue<string>();
        using var database = new LiteDatabase(BlockChainDb);
        var blocksRepository = new BlocksRepository(database);
        var utxosRepository = new UtxosRepository(database);
        var wallet = Wallet.LoadFrom(PrivateKeyPath, PublicKeyPath);

        var node = new WalletNode(Address, Dns, wallet, blocksRepository, utxosRepository, confirmedTransactions);
        node.Run();
        node.SendPackageToDns();
        
        var commands = GetConsoleCommands(node);
        
        while (true)
        {
            while (confirmedTransactions.Count > 0)
                Console.WriteLine($"The transaction {confirmedTransactions.Dequeue()} has been confirmed");
            
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
            new SendCommand(node)
        };
        
        var help = new HelpCommand(commands);
        commands.Add(help);

        return commands.ToDictionary(command => command.Name, command => command);
    }
}