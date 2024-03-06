using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpMakeMasterService : ISfpMakeMasterService
    {
        private readonly ISfpDataRepository<SfpMakeMaster> _sfpMakeMasterRepository;
        public SfpMakeMasterService(ISfpDataRepository<SfpMakeMaster> sfpMakeMasterRepository)
        {
            _sfpMakeMasterRepository = sfpMakeMasterRepository;
        }
        public async Task<IEnumerable<SfpMakeMaster>> GetMakes()
        {
            try
            {
                return await _sfpMakeMasterRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpMakeMaster> GetMake(int id)
        {
            try
            {
                var data = await _sfpMakeMasterRepository.GetAsync(x=>x.Id == id);
                if (data != null && data.Count() > 0)
                    return data.First();
                else
                    return new SfpMakeMaster();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveMake(SfpMakeMaster sfpMakeMaster)
        {
            try
            {
                await _sfpMakeMasterRepository.CreateAsync(sfpMakeMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
