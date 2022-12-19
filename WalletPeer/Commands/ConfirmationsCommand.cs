using System;

namespace WalletPeer.Commands;

public class ConfirmationsCommand : ICommand
{
    private readonly WalletNode node;

    public ConfirmationsCommand(WalletNode node)
    {
        this.node = node;
    }
    
    public string Name => "conf";

    public string Description => "Shows confirmation of a recent transaction";
    
    public void Execute()
    {
        Console.Write("Input transaction hash: ");
        
        var hash = Console.ReadLine();
        if (string.IsNullOrEmpty(hash))
            return;

        var number = node.GetConfirmationsNumberByTransactionHash(hash.Trim());
        Console.WriteLine(number > 0 ? $"Confirmed by {number} nodes" : "Rejected");
    }
}