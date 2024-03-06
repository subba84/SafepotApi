using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpCustomerAbsentService : ISfpCustomerAbsentService
    {
        private readonly ISfpDataRepository<SfpCustomerAbsent> _custAbsentRepository;

        public SfpCustomerAbsentService(ISfpDataRepository<SfpCustomerAbsent> custAbsentRepository)
        {
            _custAbsentRepository = custAbsentRepository;
        }
        public async Task DeleteCustomerAbsentData(int id)
        {
            try
            {
                var masterdata = await _custAbsentRepository.GetAsync(x => x.Id == id);
                if (masterdata != null && masterdata.Count() > 0)
                    await _custAbsentRepository.DeleteAsync(masterdata.First());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpCustomerAbsent> GetCustomerAbsentData(int id)
        {
            try
            {
                var masterdata = await _custAbsentRepository.GetAsync(x => x.Id == id);
                if (masterdata != null && masterdata.Count() > 0)
                    return masterdata.First();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new SfpCustomerAbsent();
        }

        public async Task<IEnumerable<SfpCustomerAbsent>> GetCustomerAbsentDatabasedonCustomerandAgent(int customerId,int agentId)
        {
            try
            {
                var absentData = await _custAbsentRepository.GetAsync(x => x.CustomerId == customerId && x.AgentId == agentId);
                if (absentData != null && absentData.Count() > 0)
                    return absentData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpCustomerAbsent>();
        }

        public async Task<IEnumerable<SfpCustomerAbsent>> GetAbsentDatabasedonCustomer(int customerid, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                if (fromDate != null && toDate != null)
                {
                    var masterdata = await _custAbsentRepository.GetAsync(x => x.CustomerId == customerid && fromDate.Value.Date >= (x.AbsentFrom == null ? x.AbsentFrom : x.AbsentFrom.Value.Date) && toDate.Value.Date <= (x.AbsentTo == null ? x.AbsentTo : x.AbsentTo.Value.Date));
                    return masterdata;
                }
                else
                {
                    var masterdata = await _custAbsentRepository.GetAsync(x => x.CustomerId == customerid);
                    return masterdata;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomerAbsent>> GetAbsentDatabasedonAgent(int agentId,DateTime? fromDate,DateTime? toDate)
        {
            try
            {
                if(fromDate!=null && toDate != null)
                {
                    var masterdata = await _custAbsentRepository.GetAsync(x => x.AgentId == agentId && fromDate.Value.Date >= (x.AbsentFrom == null ? x.AbsentFrom : x.AbsentFrom.Value.Date) && toDate.Value.Date <= (x.AbsentTo == null ? x.AbsentTo : x.AbsentTo.Value.Date));
                    return masterdata;
                }
                else
                {
                    var masterdata = await _custAbsentRepository.GetAsync(x => x.AgentId == agentId);
                    return masterdata;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomerAbsent>> GetCustomerAbsentsData()
        {
            try
            {
                var masterdata = await _custAbsentRepository.GetAsync();
                if (masterdata != null && masterdata.Count() > 0)
                    return masterdata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpCustomerAbsent>();
        }

        public async Task SaveCustomerAbsentData(SfpCustomerAbsent sfpCustomerAbsent)
        {
            try
            {
                await _custAbsentRepository.CreateAsync(sfpCustomerAbsent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateCustomerAbsentData(SfpCustomerAbsent sfpCustomerAbsent)
        {
            try
            {
                await _custAbsentRepository.UpdateAsync(sfpCustomerAbsent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
