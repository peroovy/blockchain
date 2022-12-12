using System;
using Core;

namespace Peer.Commands;

public class GetBalanceCommand : ICommand
{
    private readonly BlockChain blockChain;
    private readonly string address;

    public GetBalanceCommand(BlockChain blockChain, string address)
    {
        this.blockChain = blockChain;
        this.address = address;
    }
    
    public void Execute()
    {
        Console.WriteLine(blockChain.GetBalance(address));
    }
}