using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpCutoffTimeMasterService : ISfpCutoffTimeMasterService
    {
        private readonly ISfpDataRepository<SfpCutoffTimeMaster> _cutoffTimeRepository;

        public SfpCutoffTimeMasterService(ISfpDataRepository<SfpCutoffTimeMaster> cutoffTimeRepository)
        {
            _cutoffTimeRepository = cutoffTimeRepository;
        }

        public async Task<IEnumerable<SfpCutoffTimeMaster>> GetSfpCutoffTimeMasters()
        {
            try
            {
                return await _cutoffTimeRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<SfpCutoffTimeMaster> GetCutoffTimebasedonAgent(int agentId)
        {
            try
            {
                var data = await _cutoffTimeRepository.GetAsync(x=>x.AgentId == agentId);
                if (data == null)
                    return new SfpCutoffTimeMaster();
                return data.Last();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<SfpCutoffTimeMaster> GetCutoffTimeData(int id)
        {
            try
            {
                var results = await _cutoffTimeRepository.GetAsync(x => x.Id == id);
                if (results != null && results.Count() > 0)
                    return results.First();
                else
                    return new SfpCutoffTimeMaster();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> CreateCutoffTimeData(SfpCutoffTimeMaster sfpCutoffTimeMaster)
        {
            try
            {
                return await _cutoffTimeRepository.CreateAsync(sfpCutoffTimeMaster);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateCutoffTimeData(SfpCutoffTimeMaster sfpCutoffTimeMaster)
        {
            try
            {
                await _cutoffTimeRepository.UpdateAsync(sfpCutoffTimeMaster);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteCutoffTimeData(int id)
        {
            try
            {
                var data = await _cutoffTimeRepository.GetAsync(x => x.Id == id);
                if (data != null && data.Count() > 0)
                    await _cutoffTimeRepository.DeleteAsync(data.First());
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
