using System.Collections.Immutable;
using System.Linq;
using Core.Utils;

namespace Core.Transactions;

public class Transaction
{
    public Transaction(ImmutableArray<Input> inputs, ImmutableArray<Output> outputs)
    {
        Inputs = inputs;
        Outputs = outputs;
        
        var inputSumHash = string.Concat(inputs.Select(input => input.Hash));
        var outputSumHash = string.Concat(outputs.Select(output => output.Hash));
        Hash = Hashing
            .SumSHA256(inputSumHash, outputSumHash)
            .ToHexDigest();
    }
    
    public string Hash { get; }
    
    public ImmutableArray<Input> Inputs { get; }
    
    public ImmutableArray<Output> Outputs { get; }

    public static Transaction CreateCoinbase(string address, int subsidy)
    {
        return new Transaction(
            ImmutableArray.Create(new Input(Hashing.ZeroHash, -1, $"Reward to {address}: {subsidy}")),
            ImmutableArray.Create(new Output(subsidy, address))
        );
    }
}