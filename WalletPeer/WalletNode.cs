using System.Net;
using Core;
using Core.Network;
using Core.Transactions;
using Core.Utils;

namespace WalletPeer;

public class WalletNode : Peer
{
    public WalletNode(IPEndPoint address, IPEndPoint dns, Wallet wallet) : base(address, dns, wallet)
    {
    }

    public string Address => Wallet.Address;

    public int Balance => BlockChain.GetBalance(Wallet);

    public Transaction CreateTransaction(string receiverAddress, int amount)
    {
        var transaction = BlockChain.CreateTransaction(Wallet, receiverAddress, amount);

        var serializedTransaction = Serializer.ToBytes(transaction);
        var package = new Package(AddressFrom, PackageTypes.Transaction, serializedTransaction);
        SendBroadcast(package);
        
        return transaction;
    }
}