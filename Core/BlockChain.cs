using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core.Transactions;
using Core.Utils;
using LiteDB;

namespace Core;

public class BlockChain : IEnumerable<Block>
{
    private readonly LiteDatabase database = new("blockchain.db");
    private readonly ILiteCollection<Block> blocksCollection;
    private readonly ILiteCollection<Transaction> transactionsCollection;
    private readonly ILiteCollection<Input> inputsCollection;
    private readonly ILiteCollection<Output> outputsCollection;

    private const int Subsidy = 60;
    private const int Difficult = 4;

    public BlockChain()
    {
        blocksCollection = database
            .GetCollection<Block>()
            .Include("$.Transactions[*]")
            .Include("$.Transactions[*].Inputs[*]")
            .Include("$.Transactions[*].Outputs[*]");

        transactionsCollection = database
            .GetCollection<Transaction>()
            .Include(tx => tx.Inputs)
            .Include(tx => tx.Outputs);
            
        inputsCollection = database.GetCollection<Input>();
        outputsCollection = database.GetCollection<Output>();
    }
    
    public int Height => IsEmpty ? 0 : Tail.Height;

    public bool IsEmpty => !blocksCollection.FindAll().Any();

    private Block Tail => blocksCollection
        .FindAll()
        .OrderBy(block => (block.Height, -block.Timestamp))
        .Last();

    public void TryAddBlock(Block block)
    {
        var expectedMerkleRoot = MerkleTree
            .Create(block.Transactions.Select(tx => tx.Hash))
            .Hash;
        
        if (!block.Hash.StartsWithBitsNumber(Difficult) 
            || block.PreviousBlockHash != Tail.Hash 
            || block.MerkleRoot != expectedMerkleRoot)
        {
            return;
        }

        InsertBlock(block);
    }

    public void CreateGenesis(Wallet wallet)
    {
        CreateBlock(wallet, Array.Empty<Transaction>(), isGenesis: true);
    }
    
    public Block CreateBlock(Wallet wallet, IEnumerable<Transaction> transactions, bool isGenesis = false)
    {
        Block block;
        var transactionsWithCoinbase = transactions
            .Prepend(Transaction.CreateCoinbase(wallet, Subsidy))
            .ToArray();
        
        if (isGenesis)
        {
            block = MineBlock(Hashing.ZeroHash, 0, transactionsWithCoinbase, 0);
        }
        else
        {
            var lastBlock = Tail;
            block = MineBlock(lastBlock.Hash, lastBlock.Height, transactionsWithCoinbase, Difficult);
        }

        InsertBlock(block);
        
        return block;
    }

    public int GetBalance(Wallet wallet)
    {
        return FindUtxosLockedWith(wallet.PublicKeyHash)
            .Select(utxo => utxo.Value)
            .Sum();
    }
    
    public Transaction CreateTransaction(Wallet sender, string receiverAddress, int amount)
    {
        var inputs = ImmutableArray.CreateBuilder<Input>();
        var accumulated = 0;
        
        foreach (var utxo in FindUtxosLockedWith(sender.PublicKeyHash)
                     .OrderBy(utxo => utxo.Value))
        {
            var input = new Input(utxo.Hash, sender.PublicKey);
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
    
    public void Reindex(Block[] blocks)
    {
        BeginTrans();
        
        blocksCollection.DeleteAll();
        transactionsCollection.DeleteAll();
        inputsCollection.DeleteAll();
        outputsCollection.DeleteAll();

        blocksCollection.InsertBulk(blocks);

        var transactions = blocks
            .SelectMany(block => block.Transactions)
            .ToArray();
        
        transactionsCollection.InsertBulk(transactions);
        inputsCollection.InsertBulk(transactions.SelectMany(tx => tx.Inputs));
        outputsCollection.InsertBulk(transactions.SelectMany(tx => tx.Outputs));
        
        EndTrans();
    }

    public IEnumerator<Block> GetEnumerator()
    {
        var last = Tail;

        do
        {
            yield return last;

            last = blocksCollection.FindOne(block => block.Hash == last.PreviousBlockHash);
            
        } while (last is not null);
    }
    
    private static Block MineBlock(string previousBlockHash, int previousHeight, Transaction[] transactions, int difficult)
    {
        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

        if (transactions.Any(transaction => !transaction.IsCoinbase && !transaction.VerifiedSignature))
            throw new InvalidTransactionException();

        for (var nonce = 0L; nonce < long.MaxValue; nonce++)
        {
            var block = new Block(previousBlockHash, previousHeight + 1, timestamp, transactions, difficult, nonce);

            if (block.Hash.StartsWithBitsNumber(difficult))
                return block;
        }

        throw new InvalidOperationException("Not found nonce");
    }

    private IEnumerable<Output> FindUtxosLockedWith(string publicKeyHash)
    {
        return outputsCollection.Find(output => !output.IsSpent && output.PublicKeyHash == publicKeyHash);
    }
    
    private void BeginTrans()
    {
        if (!database.BeginTrans())
            throw new InvalidOperationException("Found uncommited transaction in current thread");
    }

    private void EndTrans()
    {
        if (!database.Commit())
            throw new InvalidOperationException("Transaction was not be started");
    }

    private void InsertBlock(Block block)
    {
        var spentUtxosHashes = block
            .Transactions
            .SelectMany(tx => tx.Inputs)
            .Select(input => input.OutputHash)
            .ToHashSet();
        
        BeginTrans();
        
        blocksCollection.Insert(block);
        transactionsCollection.InsertBulk(block.Transactions);
        inputsCollection.InsertBulk(block.Transactions.SelectMany(tx => tx.Inputs));
        outputsCollection.InsertBulk(block.Transactions.SelectMany(tx => tx.Outputs));

        var outputs = outputsCollection
            .Find(output => spentUtxosHashes.Contains(output.Hash))
            .Select(output =>
            {
                output.IsSpent = true;
                return output;
            });
        outputsCollection.Update(outputs);
        
        EndTrans();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
