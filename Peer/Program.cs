using System;
using Core;

namespace Peer;


internal class Program
{
    public static void Main(string[] args)
    {
        var blockchain = new BlockChain();
        
        blockchain.Add("Yura send 10 YOX to Nikita", 20);
        
        foreach (var block in blockchain.Blocks)
        {
            Console.WriteLine($"Previous Hash: {block.PreviousHash}");
            Console.WriteLine($"Hash: {block.Hash}");
            Console.WriteLine($"Timestamp: {block.Timestamp}");
            Console.WriteLine($"Data: {block.Data}");
            Console.WriteLine($"Nonce: {block.Nonce}");
            Console.WriteLine();
        }
    }
}