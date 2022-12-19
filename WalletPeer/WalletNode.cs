using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<string, int> confirmedTransaction = new();
    
    public WalletNode(IPEndPoint address, IPEndPoint dns, 
        Wallet wallet, IBlocksRepository blocksRepository, IUtxosRepository utxosRepository) 
        : base(address, dns, wallet, blocksRepository, utxosRepository)
    {
    }

    public string Address => Wallet.Address;
    
    public int Balance => BlockChain.GetBalance(Wallet.PublicKeyHash);

    public int GetConfirmationsNumberByTransactionHash(string hash)
    {
        return !confirmedTransaction.TryGetValue(hash, out var number) ? 0 : number;
    }
    
    public Transaction CreateTransaction(string receiverAddress, int amount)
    {
        var transaction = BlockChain.CreateTransaction(Wallet, receiverAddress, amount);

        var serializedTransaction = Serializer.ToBytes(transaction);
        var package = new Package(AddressFrom, PackageTypes.Transaction, serializedTransaction);
        foreach (var address in Addresses.Keys)
            Send(address, package);
        
        return transaction;
    }
    
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

    private void HandleTransactionConfirmation(Package package)
    {
        var hash = Encoding.UTF8.GetString(package.Body);

        if (!confirmedTransaction.ContainsKey(hash))
            confirmedTransaction[hash] = 0;

        confirmedTransaction[hash] += 1;
    }
}