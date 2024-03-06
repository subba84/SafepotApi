using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpUomMasterService
    {
        Task<IEnumerable<SfpUomMaster>> GetUoms();
        Task<SfpUomMaster> GetUom(int id);
        Task SaveUom(SfpUomMaster sfpUomMaster);
        Task ClearData();
    }
}
