using System;

namespace WalletPeer.Commands;

public class BalanceCommand : ICommand
{
    private readonly WalletNode node;
    
    public string Name => "balance";

    public string Description => "Search for unspent transaction outputs";

    public BalanceCommand(WalletNode node)
    {
        this.node = node;
    }

    public void Execute()
    {
        Console.WriteLine(node.Balance);
    }
}