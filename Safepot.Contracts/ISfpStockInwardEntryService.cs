using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpStockInwardEntryService
    {
        public Task<IEnumerable<SfpInwardStockEntry>> GetStockEntries(int agentId);

        public Task<SfpInwardStockEntry> GetStockEntry(int id);

        public Task<int> SaveStockEntry(SfpInwardStockEntry sfpInwardStockEntry);

        public Task UpdateStockEntry(SfpInwardStockEntry sfpInwardStockEntry);

        public Task DeleteStockEntry(SfpInwardStockEntry sfpInwardStockEntry);
    }
}
