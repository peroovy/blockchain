using System;
using System.Collections.Generic;
using Core;
using Core.Repositories.LiteDB;
using Core.Transactions;
using LiteDB;
using Peer.Commands;

namespace Peer;

internal static class Program
{
    private const string PeerAddress = "nikita";

    private const string DbPath = "blockchain.db";
        
    public static void Main(string[] args)
    {
        using var database = new LiteDatabase(DbPath);
        var blocksRepository = new BlocksRepository(database);
        var blockChain = new BlockChain(blocksRepository, PeerAddress);
        var transactions = new List<Transaction>();
        
        var commands = new Dictionary<string, ICommand>
        {
            ["blocks"] = new PrintBlockChainCommand(blockChain),
            ["mine"] = new AddBlockCommand(blockChain, transactions),
            ["balance"] = new GetBalanceCommand(blockChain, PeerAddress),
            ["tx"] = new AddTransactionToPoolCommand(blockChain, transactions, PeerAddress)
        };

        while (true)
        {
            Console.Write(">> ");
            var commandName = Console.ReadLine();

            if (!commands.TryGetValue(commandName, out var command))
            {
                Console.WriteLine("ERROR: Unknown command");
                continue;
            }
            
            command.Execute();
        }
    }
}