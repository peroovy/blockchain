using System;
using Core;
using Core.Utils;

namespace Peer.Commands;

public class GetBalanceCommand : ICommand
{
    private readonly Wallet wallet;
    private readonly BlockChain blockChain;

    public GetBalanceCommand(Wallet wallet, BlockChain blockChain)
    {
        this.wallet = wallet;
        this.blockChain = blockChain;
    }
    
    public void Execute()
    {
        Console.Write("Input address (skip if your address): ");
        var address = Console.ReadLine();

        if (string.IsNullOrEmpty(address))
        {
            Console.WriteLine(blockChain.GetBalance(wallet.PublicKeyHash));
            return;
        }

        if (RsaUtils.ValidateAddress(address))
        {
            Console.WriteLine(blockChain.GetBalance(RsaUtils.GetPublicKeyHashFromAddress(address)));
            return;
        }
        
        Console.WriteLine("ERROR: Wrong address");
    }
}