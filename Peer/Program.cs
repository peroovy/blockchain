using System;
using System.Collections.Generic;
using System.IO;
using Core;
using Core.Repositories.LiteDB;
using Core.Transactions;
using LiteDB;
using Peer.Commands;

namespace Peer;

internal static class Program
{
    private const string DbPath = "blockchain.db";
    private const string PublicKeyPath = "wallet.pub";
    private const string PrivateKeyPath = "wallet";

    public static void Main(string[] args)
    {
        using var database = new LiteDatabase(DbPath);

        var wallet = LoadWallet();
        
        var blocksRepository = new BlocksRepository(database);
        var blockChain = new BlockChain(blocksRepository, wallet);
        var transactions = new List<Transaction>();
        
        var commands = new Dictionary<string, ICommand>
        {
            ["blocks"] = new PrintBlockChainCommand(blockChain),
            ["mine"] = new AddBlockCommand(blockChain, transactions),
            ["balance"] = new GetBalanceCommand(wallet, blockChain),
            ["send"] = new SendCurrencyCommand(wallet, blockChain, transactions)
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
    
    private static Wallet LoadWallet()
    {
        var sshFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
        
        var privateKey = File.ReadAllText(Path.Combine(sshFolder, PrivateKeyPath));
        var publicKey = File.ReadAllText(Path.Combine(sshFolder, PublicKeyPath));

        return new Wallet(privateKey, publicKey);
    }
}