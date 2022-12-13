using System;
using Core;

namespace Peer.Commands;

public class PrintBlockChainCommand : ICommand
{
    private readonly BlockChain blockChain;

    public PrintBlockChainCommand(BlockChain blockChain)
    {
        this.blockChain = blockChain;
    }
    
    public void Execute()
    {
        foreach (var block in blockChain)
        {
            Console.WriteLine($"Previous Hash: {block.PreviousBlockHash}");
            Console.WriteLine($"Hash: {block.Hash}");
            Console.WriteLine($"Timestamp: {block.Timestamp}");
            Console.WriteLine($"Nonce: {block.Nonce}");
            Console.WriteLine($"Difficult: {block.Difficult}");
            Console.WriteLine();
        }
    }
}