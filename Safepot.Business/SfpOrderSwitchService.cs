using Microsoft.VisualBasic;
using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Safepot.Business
{
    public class SfpOrderSwitchService : ISfpOrderSwitchService
    {
        private readonly ISfpDataRepository<SfpOrderSwitch> _repository;
        public SfpOrderSwitchService(ISfpDataRepository<SfpOrderSwitch> repository)
        {
            _repository = repository;
        }

        public async Task<SfpOrderSwitch> GetOrderSwitchDetails(int agentId, int customerId)
        {
            try
            {
                var agentCustomerSwitch = await _repository.GetAsync(x => x.AgentId == agentId && x.CustomerId == customerId);
                if (agentCustomerSwitch != null && agentCustomerSwitch.Count() > 0)
                {
                    return agentCustomerSwitch.First();
                }
                return new SfpOrderSwitch();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> IsOrderGenerationOff(int agentId, int customerId)
        {
            try
            {
                var agentCustomerSwitch = await _repository.GetAsync(x => x.AgentId == agentId && x.CustomerId == customerId);
                if (agentCustomerSwitch != null && agentCustomerSwitch.Count() > 0)
                {
                    if (agentCustomerSwitch.First().IsOrderGenerationOff == true)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SwitchOrderGeneration(SfpOrderSwitch sfpOrderSwitch)
        {
            try
            {
                if(sfpOrderSwitch.IsOrderGenerationOff == true) 
                {   
                    await _repository.CreateAsync(sfpOrderSwitch);
                }
                else
                {
                    var agentCustomerSwitch = await _repository.GetAsync(x=>x.AgentId == sfpOrderSwitch.AgentId && x.CustomerId == sfpOrderSwitch.CustomerId);
                    if(agentCustomerSwitch != null && agentCustomerSwitch.Count() > 0) 
                    {
                        await _repository.DeleteAsync(agentCustomerSwitch.First());
                    }
                }                
            }
            catch(Exception ex) 
            {
                throw ex;
            }
        }
    }
}
