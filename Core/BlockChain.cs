using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Repositories;
using Core.Transactions;
using Core.Utils;

namespace Core;

public class BlockChain
{
    private readonly IBlocksRepository blocksRepository;
    private readonly IUtxosRepository utxosRepository;

    private const int Subsidy = 60;
    private const int Difficult = 3;

    public BlockChain(Wallet peerWallet, IBlocksRepository blocksRepository, IUtxosRepository utxosRepository)
    {
        this.blocksRepository = blocksRepository;
        this.utxosRepository = utxosRepository;

        if (blocksRepository.ExistsAny())
            return;

        MineBlock(peerWallet, Enumerable.Empty<Transaction>(), isFirstBlock: true);
    }

    public Block MineBlock(Wallet wallet, IEnumerable<Transaction> transactions, bool isFirstBlock = false)
    {
        var block = MineBlock(isFirstBlock ? Hashing.ZeroHash : blocksRepository.Last().Hash, 
            transactions.Prepend(Transaction.CreateCoinbase(wallet, Subsidy)).ToArray(), 
            Difficult);

        blocksRepository.Add(block);

        foreach (var input in block
                     .Transactions
                     .Where(transaction => !transaction.IsCoinbase)
                     .SelectMany(transaction => transaction.Inputs))
        {
            utxosRepository.DeleteOne(input.PreviousTransactionHash, input.OutputIndex);
        }

        var utxos = block
            .Transactions
            .SelectMany(transaction => transaction.Outputs.Select((output, i) => new Utxo(transaction.Hash, i, output)));

        utxosRepository.InsertBulk(utxos);

        return block;
    }

    public int GetBalance(string publicKeyHash)
    {
        return FindLockedUtxosWith(publicKeyHash)
            .Select(utxo => utxo.Output.Value)
            .Sum();
    }
    
    public Transaction CreateTransaction(Wallet sender, string receiverAddress, int amount)
    {
        var inputs = new List<Input>();
        var accumulated = 0;
        foreach (var utxo in FindLockedUtxosWith(sender.PublicKeyHash).OrderBy(utxo => utxo.Output.Value))
        {
            var input = new Input(utxo.TransactionHash, utxo.OutputIndex, sender.PublicKey);
            inputs.Add(input);
            
            accumulated += utxo.Output.Value;

            if (accumulated >= amount)
                break;
        }

        if (accumulated < amount)
            throw new NotEnoughCurrencyException();

        var outputs = new List<Output> { new(amount, RsaUtils.GetPublicKeyHashFromAddress(receiverAddress)) };
        if (accumulated > amount)
            outputs.Add(new Output(accumulated - amount, sender.PublicKeyHash));

        var transaction = new Transaction(inputs, outputs);
        var signature = RsaUtils.SignData(sender.PrivateKey, transaction.Hash);

        foreach (var input in inputs)
            input.Signature = signature;

        return transaction;
    }
    
    private IEnumerable<Utxo> FindLockedUtxosWith(string publicKeyHash)
    {
        return utxosRepository.Filter(utxo => utxo.Output.IsLockedWith(publicKeyHash));
    }

    private static Block MineBlock(string previousBlockHash, IReadOnlyList<Transaction> transactions, int difficult)
    {
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

        if (transactions.Any(transaction => !transaction.IsCoinbase && !transaction.VerifySignature()))
            throw new InvalidTransactionException();

        for (var nonce = 0L; nonce < long.MaxValue; nonce++)
        {
            var block = new Block(previousBlockHash, timestamp, transactions, difficult, nonce);

            if (ValidateBlock(block, difficult))
                return block;
        }

        throw new InvalidOperationException("Not found nonce");
    }

    private static bool ValidateBlock(Block block, int expectedDifficult)
    {
        return block.Hash
            .ToBits()
            .Take(expectedDifficult)
            .All(bit => !bit);
    }
}
