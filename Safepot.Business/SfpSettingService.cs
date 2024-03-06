using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpSettingService : ISfpSettingService
    {
        private readonly ISfpDataRepository<SfpSetting> _sfpSettingRepository;
        public SfpSettingService(ISfpDataRepository<SfpSetting> sfpSettingRepository)
        {
            _sfpSettingRepository = sfpSettingRepository;
        }

        public async Task SaveSetting(SfpSetting sfpSetting)
        {
            try
            {
                var agentData = await _sfpSettingRepository.GetAsync(x => x.AgentId == sfpSetting.AgentId);
                if (agentData == null || agentData.Count() == 0)
                {
                    await _sfpSettingRepository.CreateAsync(sfpSetting);
                }
                else
                {
                    sfpSetting.Id = agentData.First().Id;
                    await _sfpSettingRepository.UpdateAsync(sfpSetting);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpSetting> GetSettingforAgent(int agentId)
        {
            try
            {
                var agentData = await _sfpSettingRepository.GetAsync(x => x.AgentId == agentId);
                if (agentData != null && agentData.Count() > 0)
                    return agentData.First();
                return new SfpSetting();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        } 
    }
}
