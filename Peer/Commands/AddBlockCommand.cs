using System;
using System.Collections.Generic;
using Core;
using Core.Transactions;

namespace Peer.Commands;

public class AddBlockCommand : ICommand
{
    private readonly BlockChain blockChain;
    private readonly List<Transaction> transactions;

    public AddBlockCommand(BlockChain blockChain, List<Transaction> transactions)
    {
        this.blockChain = blockChain;
        this.transactions = transactions;
    }
    
    public void Execute()
    {
        blockChain.AddBlock(transactions);
        transactions.Clear();

        Console.WriteLine("Success!");
    }
}