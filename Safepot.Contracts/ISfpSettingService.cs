using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpSettingService
    {
        Task SaveSetting(SfpSetting sfpSetting);
        Task<SfpSetting> GetSettingforAgent(int agentId);
    }
}
