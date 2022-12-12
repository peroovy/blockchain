using System;
using System.Collections.Immutable;
using Core;
using Core.Transactions;

namespace Peer.Commands;

public class AddBlockCommand : ICommand
{
    private readonly BlockChain blockChain;
    private readonly string address;
    private readonly int subsidy;

    public AddBlockCommand(BlockChain blockChain, string address, int subsidy)
    {
        this.blockChain = blockChain;
        this.address = address;
        this.subsidy = subsidy;
    }
    
    public void Execute()
    {
        blockChain.AddBlock(ImmutableArray.Create(Transaction.CreateCoinbase(address, subsidy)));
        
        Console.WriteLine("Success!");
    }
}