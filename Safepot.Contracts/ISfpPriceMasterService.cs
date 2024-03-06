using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpPriceMasterService
    {
        Task<IEnumerable<SfpPriceMaster>> GetPriceMasterData();
        Task CreatePriceMaster(SfpPriceMaster sfpPriceMaster);
        Task UpdatePriceMaster(SfpPriceMaster sfpPriceMaster);
        Task DeletePriceMaster(int id);
    }
}
