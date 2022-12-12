using System;
using System.Collections.Generic;
using Core;
using Core.Repositories.LiteDB;
using LiteDB;
using Peer.Commands;

namespace Peer;

internal static class Program
{
    
    
    public static void Main(string[] args)
    {
        using var database = new LiteDatabase("blockchain.db");
        var blocksRepository = new BlocksRepository(database);
        var blockChain = new BlockChain(blocksRepository, "nikita");
        
        var commands = new Dictionary<string, ICommand>
        {
            ["blocks"] = new PrintBlockChainCommand(blockChain),
            ["mine"] = new AddBlockCommand(blockChain, "nikita", 100),
            ["balance"] = new GetBalanceCommand(blockChain, "nikita")
        };

        while (true)
        {
            Console.Write(">> ");
            var commandName = Console.ReadLine();

            if (!commands.TryGetValue(commandName, out var command))
            {
                Console.WriteLine("Unknown command");
                continue;
            }
            
            command.Execute();
        }
    }
}