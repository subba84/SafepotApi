using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpAgentCustDeliveryMapService
    {
        public Task<IEnumerable<SfpUser>> GetAgentAssociatedCustomers(int agentid);
        public Task<IEnumerable<SfpUser>> GetAgentAssociatedDeliveryBoys(int agentid);
        public Task<IEnumerable<SfpUser>> GetCustomerAssociatedDeliveryBoys(int customerid);
        public Task<IEnumerable<SfpUser>> GetCustomerAssociatedAgents(int customerid);
        public Task<IEnumerable<SfpUser>> GetDeliveryAssociatedAgents(int deliveryid);
        public Task<IEnumerable<SfpUser>> GetDeliveryAssociatedCustomers(int deliveryid);
        public Task<IEnumerable<SfpAgentCustDeliveryMap>> GetAllMappings();
        public Task<IEnumerable<SfpAgentCustDeliveryMap>> GetAllMappingsbasedonAgent(int agentId);
        public Task<SfpAgentCustDeliveryMap> GetMapping(int id);
        public Task<int> SaveMapping(SfpAgentCustDeliveryMap sfpAgentCustDeliveryMap);
        public Task UpdateMapping(SfpAgentCustDeliveryMap sfpAgentCustDeliveryMap);
        public Task DeleteMapping(SfpAgentCustDeliveryMap sfpAgentCustDeliveryMap);
        public Task<IEnumerable<SfpUser>> GetFreeCustomersbasedonAgentandDelivery(int agentId, int deliveryId);
        public Task<IEnumerable<SfpUser>> GetFreeAgentsbasedoCustomer(int customerId);
        public Task<IEnumerable<SfpUser>> GetAssociatedCustomersbasedonAgentandDelivery(int agentId, int deliveryId);
        public Task<IEnumerable<SfpUser>> GetAssociatedDeliveryBoysbasedonAgentandCustomer(int agentId, int customerId);
    }
}
