using System;

namespace WalletPeer.Commands;

public class BalanceCommand : ICommand
{
    public string Name => "balance";

    public string Description => "Search for unspent transaction outputs";

    public void Execute()
    {
        Console.WriteLine("balance");
    }
}