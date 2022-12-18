using System;

namespace WalletPeer.Commands;

public class SendCommand : ICommand
{
    public string Name => "send";

    public string Description => "Сreating an unconfirmed transaction and storing to the mempool";

    public void Execute()
    {
        Console.WriteLine("send");
    }
}