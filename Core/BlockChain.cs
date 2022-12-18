using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core.Repositories;
using Core.Transactions;
using Core.Utils;

namespace Core;

public class BlockChain
{
    private readonly IBlocksRepository blocksRepository;
    private readonly IUtxosRepository utxosRepository;

    private const int GenesisSubsidy = 100;
    private const int GenesisDifficult = 0;
    
    public BlockChain(IBlocksRepository blocksRepository, IUtxosRepository utxosRepository)
    {
        this.blocksRepository = blocksRepository;
        this.utxosRepository = utxosRepository;
    }

    public void CreateGenesis(Wallet wallet)
    {
        MineBlock(wallet, Array.Empty<Transaction>(), GenesisSubsidy, GenesisDifficult, isGenesis: true);
    }

    public (Block block, Transaction[], Utxo[] utxos, Input[] inputs) MineBlock(
        Wallet wallet, IEnumerable<Transaction> transactions, int subsidy, int difficult, bool isGenesis = false)
    {
        Block block;
        var transactionsWithCoinbase = transactions
            .Prepend(Transaction.CreateCoinbase(wallet, subsidy))
            .ToArray();
        
        if (isGenesis)
        {
            block = MineBlock(Hashing.ZeroHash, 0, transactionsWithCoinbase, difficult);
        }
        else
        {
            var lastBlock = blocksRepository.GetLast();
            block = MineBlock(lastBlock.Hash, lastBlock.Height, transactionsWithCoinbase, difficult);
        }

        blocksRepository.Insert(block);

        var inputs = block
            .Transactions
            .SelectMany(transaction => transaction.Inputs)
            .ToArray();

        foreach (var input in inputs)
            utxosRepository.DeleteOneIfExists(input.PreviousTransactionHash, input.OutputIndex);

        var utxos = block
            .Transactions
            .SelectMany(transaction => transaction.Outputs.Select((output, i) =>
                new Utxo(transaction.Hash, i, output.Value, output.PublicKeyHash)))
            .ToArray();

        utxosRepository.InsertBulk(utxos);

        return (block, transactionsWithCoinbase, utxos, inputs);
    }

    public int GetBalance(string publicKeyHash)
    {
        return utxosRepository
            .FindUtxosLockedWith(publicKeyHash)
            .Select(utxo => utxo.Value)
            .Sum();
    }
    
    public Transaction CreateTransaction(Wallet sender, string receiverAddress, int amount)
    {
        var inputs = ImmutableArray.CreateBuilder<Input>();
        var accumulated = 0;
        
        foreach (var utxo in utxosRepository
                     .FindUtxosLockedWith(sender.PublicKeyHash)
                     .OrderBy(utxo => utxo.Value))
        {
            var input = new Input(utxo.TransactionHash, utxo.Index, sender.PublicKey);
            inputs.Add(input);
            
            accumulated += utxo.Value;

            if (accumulated >= amount)
                break;
        }

        if (accumulated < amount)
            throw new NotEnoughCurrencyException();

        var outputs = ImmutableArray.CreateBuilder<Output>();
        outputs.Add(new Output(amount, RsaUtils.GetPublicKeyHashFromAddress(receiverAddress)));
        
        if (accumulated > amount)
            outputs.Add(new Output(accumulated - amount, sender.PublicKeyHash));

        var transaction = new Transaction(inputs.ToImmutable(), outputs.ToImmutable());
        var signature = RsaUtils.SignData(sender.PrivateKey, transaction.Hash);

        foreach (var input in inputs)
            input.Signature = signature;

        return transaction;
    }

    private static Block MineBlock(string previousBlockHash, int previousHeight, Transaction[] transactions, int difficult)
    {
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

        if (transactions.Any(transaction => !transaction.IsCoinbase && !transaction.VerifySignature()))
            throw new InvalidTransactionException();

        for (var nonce = 0L; nonce < long.MaxValue; nonce++)
        {
            var block = new Block(previousBlockHash, previousHeight + 1, timestamp, transactions, difficult, nonce);

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
