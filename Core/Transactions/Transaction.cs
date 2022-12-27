using System;
using System.Collections.Immutable;
using System.Linq;
using Core.Utils;
using LiteDB;

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
            .SumSha256(inputSumHash, outputSumHash, DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString())
            .ToHexDigest();
    }
    
    [BsonCtor]
    public Transaction() {}

    [BsonId]
    public int Id { get; set; }
    
    public string Hash { get; set; }
    
    [BsonRef]
    public Input[] Inputs { get; set; }
    
    [BsonRef]
    public Output[] Outputs { get; set; }
    
    public bool IsCoinbase { get; set; }
    
    [BsonIgnore]
    public bool VerifiedSignature => Inputs.All(input => RsaUtils.VerifyData(input.PublicKey, input.Signature, Hash));
    
    [BsonIgnore]
    public bool IsSelfToSelf
    {
        get
        {
            var inputHash = RsaUtils
                .HashPublicKey(Inputs[0].PublicKey)
                .ToHexDigest();

            return Outputs.All(output => output.PublicKeyHash == inputHash);
        }
    }

    public static Transaction CreateCoinbase(Wallet wallet, int subsidy)
    {
        return new Transaction(
            ImmutableArray.Create(new Input(Hashing.ZeroHash, $"Reward to {wallet.Address}: {subsidy}")),
            ImmutableArray.Create(new Output(subsidy, wallet.PublicKeyHash)),
            isCoinbase: true
        );
    }
}