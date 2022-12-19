using System.Collections.Generic;
using System.Net;
using System.Text;
using Core;
using Core.Network;
using Core.Repositories;
using Core.Transactions;
using Core.Utils;

namespace WalletPeer;

public class WalletNode : Peer
{
    private readonly Queue<string> confirmedTransactions;

    public WalletNode(IPEndPoint address, IPEndPoint dns, 
        Wallet wallet, IBlocksRepository blocksRepository, IUtxosRepository utxosRepository, Queue<string> confirmedTransactions) 
        : base(address, dns, wallet, blocksRepository, utxosRepository)
    {
        this.confirmedTransactions = confirmedTransactions;
    }

    public string Address => Wallet.Address;
    
    public int Balance => BlockChain.GetBalance(Wallet.PublicKeyHash);

    protected override void HandlePackage(Package package)
    {
        base.HandlePackage(package);

        switch (package.PackageTypes)
        {
            case PackageTypes.TransactionConfirmation:
                HandleTransactionConfirmation(package);
                break;
        }
    }

    public Transaction AddTransactionToMempool(string receiverAddress, int amount)
    {
        var transaction = BlockChain.CreateTransaction(Wallet, receiverAddress, amount);

        var serializedTransaction = Serializer.ToBytes(transaction);
        var package = new Package(AddressFrom, PackageTypes.Transaction, serializedTransaction);
        foreach (var address in Addresses.Keys)
            Send(address, package);
        
        return transaction;
    }

    private void HandleTransactionConfirmation(Package package)
    {
        var hash = Encoding.UTF8.GetString(package.Body);
        
        confirmedTransactions.Enqueue(hash);
    }
}