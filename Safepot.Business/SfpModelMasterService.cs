using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpModelMasterService : ISfpModelMasterService
    {
        private readonly ISfpDataRepository<SfpModelMaster> _sfpModelMasterRepository;
        public SfpModelMasterService(ISfpDataRepository<SfpModelMaster> sfpModelMasterRepository)
        {
            _sfpModelMasterRepository = sfpModelMasterRepository;
        }
        public async Task<IEnumerable<SfpModelMaster>> GetModels()
        {
            try
            {
                return await _sfpModelMasterRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpModelMaster> GetModel(int id)
        {
            try
            {
                var data = await _sfpModelMasterRepository.GetAsync(x => x.Id == id);
                if (data != null && data.Count() > 0)
                    return data.First();
                else
                    return new SfpModelMaster();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveModel(SfpModelMaster sfpModelMaster)
        {
            try
            {
                await _sfpModelMasterRepository.CreateAsync(sfpModelMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
