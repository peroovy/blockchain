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
        IsCoinbase = isCoinbase;
        Inputs = inputs;
        Outputs = outputs;

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

    public bool VerifySignature() => Inputs.All(input => RsaUtils.VerifyData(input.PublicKey, input.Signature, Hash));

    public static Transaction CreateCoinbase(Wallet wallet, int subsidy)
    {
        return new Transaction(
            new List<Input> { new(Hashing.ZeroHash, -1, $"Reward to {wallet.Address}: {subsidy}") },
            new List<Output> { new(subsidy, wallet.PublicKeyHash) },
            isCoinbase: true
        );
    }
}