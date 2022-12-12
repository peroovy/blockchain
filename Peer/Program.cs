using System;
using Core;
using Core.Repositories.LiteDB;
using LiteDB;

namespace Peer;


internal class Program
{
    public static void Main(string[] args)
    {
        using var database = new LiteDatabase("blockchain.db");
        var blocksRepository = new BlocksRepository(database);

        var blockchain = new BlockChain(blocksRepository, "nikita");
        
        foreach (var block in blockchain)
        {
            Console.WriteLine($"Previous Hash: {block.PreviousHash}");
            Console.WriteLine($"Hash: {block.Hash}");
            Console.WriteLine($"Timestamp: {block.Timestamp}");
            Console.WriteLine($"Nonce: {block.Nonce}");
            Console.WriteLine();
        }
    }
}