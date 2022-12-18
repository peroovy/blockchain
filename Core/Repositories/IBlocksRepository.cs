using System.Collections.Generic;
using System.Collections.Immutable;

namespace Core.Repositories;

public interface IBlocksRepository
{
    bool ExistsAny();
    
    void Insert(Block block);
    
    Block GetLast();
    
    int GetMaxHeight();
    
    void DeleteAll();
    
    void InsertBulk(IEnumerable<Block> blocks);
    
    IEnumerable<Block> GetBlockChain();
}