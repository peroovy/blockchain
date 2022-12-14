using System;
using System.Collections.Generic;
using Core;
using Core.Transactions;
using Core.Utils;

namespace Peer.Commands;

public class SendCurrencyCommand : ICommand
{
    private readonly Wallet wallet;
    private readonly BlockChain blockChain;
    private readonly List<Transaction> transactions;

    public SendCurrencyCommand(Wallet wallet, BlockChain blockChain, List<Transaction> transactions)
    {
        this.wallet = wallet;
        this.blockChain = blockChain;
        this.transactions = transactions;
    }
    
    public void Execute()
    {
        Console.Write("Input receiver address: ");
        var receiverAddress = Console.ReadLine();
        if (!RsaUtils.ValidateAddress(receiverAddress))
        {
            Console.WriteLine("ERROR: Wrong address");
            return;
        }
        
        Console.Write("Input amount: ");
        var amount = Convert.ToInt32(Console.ReadLine());

        try
        {
            var transaction = blockChain.CreateTransaction(wallet, receiverAddress, amount);
            var block = blockChain.MineBlock(wallet, new[] { transaction });
            
            Console.WriteLine($"Block: {block.Hash}");
            Console.WriteLine($"Difficult: {block.Difficult}");
        }
        catch (NotEnoughCurrencyException)
        {
            Console.WriteLine("ERROR: Not enough currency");
        }
        catch (InvalidTransactionException)
        {
            Console.WriteLine("ERROR: Invalid transaction signature");
        }
    }
}