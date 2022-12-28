using System;
using Core;
using Core.Utils;

namespace WalletPeer.Commands;

public class SendCommand : ICommand
{
    private readonly WalletNode node;

    public SendCommand(WalletNode node)
    {
        this.node = node;
    }
    
    public string Name => "send";

    public string Description => "Сreate an unconfirmed transaction and store to the mempool";

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
            var transaction = node.CreateTransaction(receiverAddress, amount);
            Console.WriteLine($"Transaction: {transaction.Hash}");
        }
        catch (NotEnoughCurrencyException)
        {
            Console.WriteLine("ERROR: Not enough currency");
        }
    }
}