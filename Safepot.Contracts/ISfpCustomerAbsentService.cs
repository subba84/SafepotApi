using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpCustomerAbsentService
    {
        public Task DeleteCustomerAbsentData(int id);
        public Task<SfpCustomerAbsent> GetCustomerAbsentData(int id);
        public Task<IEnumerable<SfpCustomerAbsent>> GetAbsentDatabasedonCustomer(int customerid,DateTime? fromDate,DateTime? toDate);
        public Task<IEnumerable<SfpCustomerAbsent>> GetAbsentDatabasedonAgent(int agentId, DateTime? fromDate, DateTime? toDate);
        public Task<IEnumerable<SfpCustomerAbsent>> GetCustomerAbsentsData();
        public Task SaveCustomerAbsentData(SfpCustomerAbsent sfpCustomerAbsent);
        public Task UpdateCustomerAbsentData(SfpCustomerAbsent sfpCustomerAbsent);
        public Task<IEnumerable<SfpCustomerAbsent>> GetCustomerAbsentDatabasedonCustomerandAgent(int customerId, int agentId);
    }
}
