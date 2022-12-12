using System;
using System.Collections.Generic;
using Core;
using Core.Transactions;

namespace Peer.Commands;

public class AddTransactionToPoolCommand : ICommand
{
    private readonly BlockChain blockChain;
    private readonly List<Transaction> transactions;
    private readonly string senderAddress;

    public AddTransactionToPoolCommand(BlockChain blockChain, List<Transaction> transactions, string senderAddress)
    {
        this.blockChain = blockChain;
        this.transactions = transactions;
        this.senderAddress = senderAddress;
    }
    
    public void Execute()
    {
        Console.Write("Input receiver address: ");
        var receiverAddress = Console.ReadLine();
        
        Console.Write("Input amount: ");
        var amount = Convert.ToInt32(Console.ReadLine());

        try
        {
            var transaction = blockChain.CreateTransaction(senderAddress, receiverAddress, amount);
            transactions.Add(transaction);
        }
        catch (NotEnoughCurrencyException)
        {
            Console.WriteLine("ERROR: Not enough currency");
        }
    }
}