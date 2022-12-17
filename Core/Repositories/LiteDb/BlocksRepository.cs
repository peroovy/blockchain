using System.Linq;
using LiteDB;

namespace Core.Repositories.LiteDB;

public class BlocksRepository : IBlocksRepository
{
    private readonly ILiteCollection<Block> blocks;
    
    public BlocksRepository(ILiteDatabase database)
    {
        blocks = database.GetCollection<Block>();
    }

    public bool ExistsAny() => blocks.FindAll().Any();

    public void Insert(Block block) => blocks.Insert(block);

    public Block GetLast()
    {
        var maxHeight = blocks.Max(block => block.Height);
        
        return blocks.FindOne(block => block.Height == maxHeight);
    }
}