using System;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;

namespace Core.Transactions;

[Serializable]
public class Transaction
{
    public Transaction(IReadOnlyList<Input> inputs, IReadOnlyList<Output> outputs, bool isCoinbase = false)
    {
        Inputs = inputs;
        Outputs = outputs;
        IsCoinbase = isCoinbase;

        var inputSumHash = string.Concat(inputs.Select(input => input.Hash));
        var outputSumHash = string.Concat(outputs.Select(output => output.Hash));
        Hash = Hashing
            .SumSha256(inputSumHash, outputSumHash)
            .ToHexDigest();
    }
    
    public string Hash { get; }
    
    public IReadOnlyList<Input> Inputs { get; }
    
    public IReadOnlyList<Output> Outputs { get; }
    
    public bool IsCoinbase { get; }

    public static Transaction CreateCoinbase(Wallet wallet, int subsidy)
    {
        return new Transaction(
            new List<Input> { new(Hashing.ZeroHash, -1, null, $"Reward to {wallet.Address}: {subsidy}") },
            new List<Output> { new(subsidy, wallet.PublicKeyHash) },
            isCoinbase: true
        );
    }
}