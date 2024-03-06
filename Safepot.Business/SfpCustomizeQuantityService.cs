using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpCustomizeQuantityService : ISfpCustomizeQuantityService
    {
        private readonly ISfpDataRepository<SfpCustomizeQuantity> _customizeQuantityRepository;

        public SfpCustomizeQuantityService(ISfpDataRepository<SfpCustomizeQuantity> customizeQuantityRepository)
        {
            _customizeQuantityRepository = customizeQuantityRepository;
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetQuantitiesforCustomer()
        {
            try
            {
                return await _customizeQuantityRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpCustomizeQuantity> GetQuantitiesforCustomer(int id)
        {
            try
            {
                var data = await _customizeQuantityRepository.GetAsync(x=>x.Id == id);
                if (data != null && data.Count() > 0)
                    return data.First();
                return new SfpCustomizeQuantity();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity)
        {
            try
            {
                await _customizeQuantityRepository.CreateAsync(sfpCustomizeQuantity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity)
        {
            try
            {
                await _customizeQuantityRepository.UpdateAsync(sfpCustomizeQuantity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity)
        {
            try
            {
                await _customizeQuantityRepository.DeleteAsync(sfpCustomizeQuantity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
