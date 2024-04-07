using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpMakeModelMasterService
    {
        Task<IEnumerable<SfpMakeModelMaster>> GetMakeModels();
        Task<IEnumerable<SfpMakeModelMaster>> GetMakeModelsbasedonAgent(int agentId);
        Task<IEnumerable<SfpMakeModelMaster>> GetExistedMakeModels(int? agentId,int? makeId, int? modelId, int? uomId, decimal qty);
        Task<SfpMakeModelMaster> GetMakeModel(int id);
        Task SaveMakeModel(SfpMakeModelMaster sfpMakeModelMaster);
        Task UpdateMakeModel(SfpMakeModelMaster sfpMakeModelMaster);
        Task DeleteMakeModel(int id);
    }
}
