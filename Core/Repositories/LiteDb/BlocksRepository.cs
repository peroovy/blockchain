using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
            Data = Serialize(block)
        };

        blocks.Insert(serialized);
    }

    public Block Last()
    {
        var serialized = blocks
            .FindAll()
            .Last();

        return Deserialize(serialized.Data);
    }

    public IEnumerable<Block> GetAll()
    {
        return blocks
            .FindAll()
            .OrderByDescending(block => block.Timestamp)
            .Select(block => Deserialize(block.Data));
    }

    private static byte[] Serialize(Block block)
    {
        var formatter = new BinaryFormatter();
        using var memoryStream = new MemoryStream();
        
        formatter.Serialize(memoryStream, block);

        return memoryStream.GetBuffer();
    }

    private static Block Deserialize(byte[] bytes)
    {
        var formatter = new BinaryFormatter();
        using var memoryStream = new MemoryStream(bytes);

        return (Block)formatter.Deserialize(memoryStream);
    }
}