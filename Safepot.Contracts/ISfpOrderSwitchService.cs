using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpOrderSwitchService
    {
        public Task SwitchOrderGeneration(SfpOrderSwitch sfpOrderSwitch);
        public Task<bool> IsOrderGenerationOff(int agentId, int customerId);
        public Task<SfpOrderSwitch> GetOrderSwitchDetails(int agentId, int customerId);
    }
}
