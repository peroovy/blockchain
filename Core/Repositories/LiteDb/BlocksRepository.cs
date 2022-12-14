using System.Collections.Generic;
using System.Linq;
using Core.Utils;
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

    public void Add(Block block)
    {
        var serialized = new SerializedBlock
        {
            Hash = block.Hash,
            Timestamp = block.Timestamp,
            Data = Serializer.ToBytes(block)
        };

        blocks.Insert(serialized);
    }

    public Block Last()
    {
        var serialized = blocks
            .FindAll()
            .Last();

        return Serializer.FromBytes<Block>(serialized.Data);
    }

    public IEnumerable<Block> GetAll()
    {
        return blocks
            .FindAll()
            .OrderBy(block => block.Timestamp)
            .Select(block => Serializer.FromBytes<Block>(block.Data));
    }
}