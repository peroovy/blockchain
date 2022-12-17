using System.Collections.Generic;

namespace Core.Repositories;

public interface IBlocksRepository
{
    bool ExistsAny();
    
    void Insert(Block block);
    
    Block GetLast();
}