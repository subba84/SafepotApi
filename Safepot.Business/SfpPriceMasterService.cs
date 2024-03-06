using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpPriceMasterService : ISfpPriceMasterService
    {
        private readonly ISfpDataRepository<SfpPriceMaster> _priceMasterRepository;

        public SfpPriceMasterService(ISfpDataRepository<SfpPriceMaster> priceMasterRepository)
        {
            _priceMasterRepository = priceMasterRepository;
        }
        public async Task CreatePriceMaster(SfpPriceMaster sfpPriceMaster)
        {
            try
            {
                await _priceMasterRepository.CreateAsync(sfpPriceMaster);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeletePriceMaster(int id)
        {
            try
            {
                var masterdata = await _priceMasterRepository.GetAsync(x => x.Id == id);
                if (masterdata != null && masterdata.Count() > 0)
                    await _priceMasterRepository.DeleteAsync(masterdata.First());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpPriceMaster>> GetPriceMasterData()
        {
            try
            {
                return await _priceMasterRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdatePriceMaster(SfpPriceMaster sfpPriceMaster)
        {
            try
            {
                await _priceMasterRepository.UpdateAsync(sfpPriceMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
