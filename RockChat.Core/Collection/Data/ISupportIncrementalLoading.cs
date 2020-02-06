using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RockChat.Core.Collection.Data
{
    public interface ISupportIncrementalLoading
    {
        Task<uint> LoadMoreItemsAsync(uint count);
        bool HasMoreItems { get; }
        bool IsLoading { get; }
    }
}
