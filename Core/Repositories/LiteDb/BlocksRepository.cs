using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace Core.Repositories.LiteDB;

public class BlocksRepository : IBlocksRepository
{
    private readonly ILiteCollection<Block> blocksCollection;
    
    public BlocksRepository(ILiteDatabase database)
    {
        blocksCollection = database.GetCollection<Block>();
    }

    public bool ExistsAny() => blocksCollection.FindAll().Any();

    public void Insert(Block block) => blocksCollection.Insert(block);

    public Block GetLast()
    {
        var maxHeight = blocksCollection.Max(block => block.Height);
        
        return blocksCollection.FindOne(block => block.Height == maxHeight);
    }

    public int GetMaxHeight() => ExistsAny() ? GetLast().Height : 0;

    public void DeleteAll() => blocksCollection.DeleteAll();

    public void InsertBulk(IEnumerable<Block> blocks) => blocksCollection.InsertBulk(blocks);

    public IEnumerable<Block> GetBlockChain()
    {
        var last = GetLast();

        do
        {
            yield return last;

            last = blocksCollection.FindOne(block => block.Hash == last.PreviousBlockHash);
            
        } while (last is not null);
    }
}