using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpCityMasterService : ISfpCityMasterService
    {
        private readonly ISfpDataRepository<SfpCityMaster> _cityRepository;

        public SfpCityMasterService(ISfpDataRepository<SfpCityMaster> cityRepository)
        {
            _cityRepository = cityRepository;
        }
        
        public async Task<IEnumerable<SfpCityMaster>> GetCities(int stateid)
        {
            try
            {
                return await _cityRepository.GetAsync(x=>x.StateId == stateid);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveCity(SfpCityMaster city)
        {
            try
            {
                await _cityRepository.CreateAsync(city);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
