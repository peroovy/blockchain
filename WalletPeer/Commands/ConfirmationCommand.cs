using System;

namespace WalletPeer.Commands;

public class ConfirmationCommand : ICommand
{
    private readonly WalletNode node;

    public ConfirmationCommand(WalletNode node)
    {
        this.node = node;
    }
    
    public string Name => "conf";

    public string Description => "Shows confirmation of a recent transaction";
    
    public void Execute()
    {
        Console.Write("Input transaction hash: ");
        var hash = Console.ReadLine();

        var confirmed = node.IsRecentTransactionConfirmed(hash);
        Console.WriteLine(confirmed ? "Confirmed" : "Rejected");
    }
}