using System;
using System.Collections.Generic;
using System.IO;
using Core;
using Core.Repositories.LiteDB;
using Core.Transactions;
using Core.Utils;
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
        using var privateFile = File.Open(PrivateKeyPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using var privateFileReader = new StreamReader(privateFile);
        using var privateFileWriter = new StreamWriter(privateFile);
        
        using var publicFile = File.Open(PublicKeyPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using var publicFileReader = new StreamReader(publicFile);
        using var publicFileWriter = new StreamWriter(publicFile);

        if (privateFile.Length == 0 || publicFile.Length == 0)
        {
            var keys = RsaUtils.GenerateRsaPair();

            privateFileWriter.Write(keys.privateKey);
            publicFileWriter.Write(keys.publicKey);
            
            return new Wallet(keys.privateKey, keys.publicKey);
        }

        var privateKey = privateFileReader.ReadToEnd();
        var publicKey = publicFileReader.ReadToEnd();

        return new Wallet(privateKey, publicKey);
    }
}