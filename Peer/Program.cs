using System;
using Core;

namespace Peer;


internal class Program
{
    public static void Main(string[] args)
    {
        var blockchain = new BlockChain();
        
        blockchain.AddBlock("sasha", 10);
        blockchain.AddBlock("nikita", 7);
        
        foreach (var block in blockchain.Blocks)
        {
            Console.WriteLine($"Previous Hash: {block.PreviousHash}");
            Console.WriteLine($"Hash: {block.Hash}");
            Console.WriteLine($"Timestamp: {block.Timestamp}");
            Console.WriteLine($"Transactions: {block.TransactionsTree.Hash}");
            Console.WriteLine($"Nonce: {block.Nonce}");
            Console.WriteLine();
        }
    }
}