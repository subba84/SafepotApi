using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpStockInwardEntryService : ISfpStockInwardEntryService
    {
        private readonly ISfpDataRepository<SfpInwardStockEntry> _stockRepository;
        public SfpStockInwardEntryService(ISfpDataRepository<SfpInwardStockEntry> stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<IEnumerable<SfpInwardStockEntry>> GetStockEntries(int agentId)
        {
            try
            {
                return await _stockRepository.GetAsync(x=>x.AgentId == agentId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpInwardStockEntry> GetStockEntry(int id)
        {
            try
            {
                var data = await _stockRepository.GetAsync(x => x.Id == id);
                if (data != null && data.Count() > 0)
                    return data.First();
                return new SfpInwardStockEntry();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> SaveStockEntry(SfpInwardStockEntry sfpInwardStockEntry)
        {
            try
            {
                return await _stockRepository.CreateAsync(sfpInwardStockEntry);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateStockEntry(SfpInwardStockEntry sfpInwardStockEntry)
        {
            try
            {
                await _stockRepository.UpdateAsync(sfpInwardStockEntry);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteStockEntry(SfpInwardStockEntry sfpInwardStockEntry)
        {
            try
            {
                await _stockRepository.DeleteAsync(sfpInwardStockEntry);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
