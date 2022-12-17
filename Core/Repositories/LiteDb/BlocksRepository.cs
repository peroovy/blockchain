using System.Linq;
using LiteDB;

namespace Core.Repositories.LiteDB;

public class BlocksRepository : IBlocksRepository
{
    private readonly ILiteCollection<SerializedBlock> blocks;
    
    public BlocksRepository(ILiteDatabase database)
    {
        blocks = database.GetCollection<SerializedBlock>();
    }

    public bool ExistsAny() => blocks.FindAll().Any();

    public void Insert(Block block)
    {
        var serializedBlock = new SerializedBlock(block.PreviousBlockHash, block.Height, block.Hash, block.Timestamp,
            block.MerkleRoot, block.Difficult, block.Nonce);

        blocks.Insert(serializedBlock);
    }

    public Block GetLast()
    {
        var maxHeight = blocks.Max(block => block.Height);
        var block = blocks.FindOne(block => block.Height == maxHeight);
        
        return new Block(
            block.PreviousBlockHash, block.Height, block.Hash, block.Timestamp, block.MerkleRoot, block.Difficult, block.Nonce
        );
    }
}