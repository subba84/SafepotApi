using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpModelMasterService
    {
        Task<IEnumerable<SfpModelMaster>> GetModels();
        Task<SfpModelMaster> GetModel(int id);
        Task SaveModel(SfpModelMaster sfpModelMaster);
    }
}
