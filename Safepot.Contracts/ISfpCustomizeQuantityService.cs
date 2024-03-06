using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpCustomizeQuantityService
    {
        public Task<IEnumerable<SfpCustomizeQuantity>> GetQuantitiesforCustomer();
        public Task<SfpCustomizeQuantity> GetQuantitiesforCustomer(int id);
        public Task SaveCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity);
        public Task UpdateCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity);
        public Task DeleteCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity);
    }
}
