using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpCutoffTimeMasterService
    {
        public Task<IEnumerable<SfpCutoffTimeMaster>> GetSfpCutoffTimeMasters();
        Task<SfpCutoffTimeMaster> GetCutoffTimebasedonAgent(int agentId);
        public Task<SfpCutoffTimeMaster> GetCutoffTimeData(int id);
        public Task<int> CreateCutoffTimeData(SfpCutoffTimeMaster sfpCutoffTimeMaster);
        public Task UpdateCutoffTimeData(SfpCutoffTimeMaster sfpCutoffTimeMaster);
        public Task DeleteCutoffTimeData(int id);
    }
}
