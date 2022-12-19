using System;
using Core;

namespace WalletPeer.Commands;

public class AddressCommand : ICommand
{
    private readonly WalletNode node;

    public AddressCommand(WalletNode node)
    {
        this.node = node;
    }
    
    public string Name => "addr";

    public string Description => "Shows your address";
    
    public void Execute()
    {
        Console.WriteLine(node.Address);
    }
}