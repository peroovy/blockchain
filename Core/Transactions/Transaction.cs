using System;
using System.Collections.Immutable;
using System.Linq;
using Core.Utils;

namespace Core.Transactions;

[Serializable]
public class Transaction
{
    public Transaction(ImmutableArray<Input> inputs, ImmutableArray<Output> outputs, bool isCoinbase = false)
    {
        Inputs = inputs.ToArray();
        Outputs = outputs.ToArray();
        IsCoinbase = isCoinbase;

        var inputSumHash = string.Concat(inputs.Select(input => input.Hash));
        var outputSumHash = string.Concat(outputs.Select(output => output.Hash));
        Hash = Hashing
            .SumSHA256(inputSumHash, outputSumHash)
            .ToHexDigest();
    }
    
    public string Hash { get; }
    
    public Input[] Inputs { get; }
    
    public Output[] Outputs { get; }
    
    public bool IsCoinbase { get; }

    public static Transaction CreateCoinbase(string address, int subsidy)
    {
        return new Transaction(
            ImmutableArray.Create(new Input(Hashing.ZeroHash, -1, $"Reward to {address}: {subsidy}")),
            ImmutableArray.Create(new Output(subsidy, address)),
            isCoinbase: true
        );
    }
}