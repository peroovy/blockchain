using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Core.Utils;

namespace Core.Transactions;

[Serializable]
public class Transaction
{
    public Transaction(ImmutableArray<Input> inputs, ImmutableArray<Output> outputs, bool isCoinbase = false)
    {
        IsCoinbase = isCoinbase;
        Inputs = inputs.ToArray();
        Outputs = outputs.ToArray();

        var inputSumHash = string.Concat(inputs.Select(input => input.Hash));
        var outputSumHash = string.Concat(outputs.Select(output => output.Hash));
        Hash = Hashing
            .SumSha256(inputSumHash, outputSumHash)
            .ToHexDigest();
    }
    
    public string Hash { get; }
    
    public Input[] Inputs { get; }
    
    public Output[] Outputs { get; }
    
    public bool IsCoinbase { get; }

    public bool VerifySignature() => Inputs.All(input => RsaUtils.VerifyData(input.PublicKey, input.Signature, Hash));

    public static Transaction CreateCoinbase(Wallet wallet, int subsidy)
    {
        return new Transaction(
            ImmutableArray.Create(new Input(Hashing.ZeroHash, -1, $"Reward to {wallet.Address}: {subsidy}")),
            ImmutableArray.Create(new Output(subsidy, wallet.PublicKeyHash)),
            isCoinbase: true
        );
    }
}