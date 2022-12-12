using System.Collections.Generic;

namespace Core.Repositories;

public interface IBlocksRepository
{
    bool ExistsAny();
    
    void Add(Block block);
    
    Block Last();
    
    IEnumerable<Block> GetAll();
}