using System.Collections.Generic;
using System.Linq;

namespace Core.Utils;

public class MerkleTree
{
    private MerkleTree(string hash, MerkleTree left = null, MerkleTree right = null)
    {
        Hash = hash;
        Left = left;
        Right = right;
    }
    
    public string Hash { get; }
    
    public MerkleTree Left { get; }
    
    public MerkleTree Right { get; }
    
    public static MerkleTree Create(IEnumerable<string> values)
    {
        var nodes = values
            .Select(value => new MerkleTree(Hashing.SumSHA256(value).ToHexDigest()))
            .ToList();

        while (nodes.Count > 1)
        {
            var nextLayer = new List<MerkleTree>();

            for (var i = 0; i < nodes.Count; i += 2)
            {
                var leftNode = nodes[i];
                var rightNode = i + 1 < nodes.Count ? nodes[i + 1] : leftNode;

                var hash = Hashing
                    .SumSHA256(leftNode.Hash, rightNode.Hash)
                    .ToHexDigest();

                var node = new MerkleTree(hash, leftNode, rightNode);
                nextLayer.Add(node);
            }

            nodes = nextLayer;
        }

        return nodes[0];
    }
}