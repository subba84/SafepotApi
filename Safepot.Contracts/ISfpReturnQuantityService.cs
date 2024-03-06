using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpReturnQuantityService
    {
        public Task DeleteReturnRequest(int id);
        public Task<SfpReturnQuantity> GetReturnRequestData(int id);
        Task<IEnumerable<SfpReturnQuantity>> GetReturnRequestsDatabasedonCustomer(int customerId, int agentId);
        public Task<IEnumerable<SfpReturnQuantity>> GetReturnRequestsData();
        public Task SaveReturnRequest(SfpReturnQuantity sfpCustomerAbsent);
        public Task UpdateReturnRequest(SfpReturnQuantity sfpReturnQuantity);
        public Task<IEnumerable<SfpCustomizeQuantity>> GetProductsbasedonAgentandCustomerforReturn(int agentId, int customerId, DateTime date);
        public Task<IEnumerable<SfpReturnQuantity>> GetReturnRequestsforCustomerApproval(int customerId, int agentId, string status);
        public Task<IEnumerable<SfpReturnQuantity>> GetReturnRequestsforAgentApproval(int customerId, int agentId, string status);
    }
}
