using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using Core.Network;
using Core.Repositories.LiteDB;
using Core.Transactions;
using Core.Utils;
using LiteDB;
using Org.BouncyCastle.Utilities.Net;
using Peer.Commands;
using IPAddress = System.Net.IPAddress;

namespace Peer;

internal static class Program
{

    public static void Main(string[] args)
    {
        // using var database = new LiteDatabase(DbPath);
        
        // var wallet = LoadWallet();
        
        // var blocksRepository = new BlocksRepository(database);
        // var utxosRepository = new UtxosRepository(database);
        
        // var blockChain = new BlockChain(wallet, blocksRepository, utxosRepository);
        // var transactions = new List<Transaction>();
        //
        // var commands = new Dictionary<string, ICommand>
        // {
        //     ["balance"] = new GetBalanceCommand(wallet, blockChain),
        //     ["send"] = new SendCurrencyCommand(wallet, blockChain, transactions)
        // };
        //
        // while (true)
        // {
        //     Console.Write(">> ");
        //     var commandName = Console.ReadLine();
        //
        //     if (!commands.TryGetValue(commandName, out var command))
        //     {
        //         Console.WriteLine("ERROR: Unknown command");
        //         continue;
        //     }
        //     
        //     command.Execute();
        // }
        
        // var node = new Node(IPAddress.Loopback, 8000);
        // node.Run();
    }
}