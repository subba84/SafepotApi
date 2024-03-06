using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpUomMasterService : ISfpUomMasterService
    {
        private readonly ISfpDataRepository<SfpUomMaster> _sfpUomMasterRepository;
        public SfpUomMasterService(ISfpDataRepository<SfpUomMaster> sfpUomMasterRepository)
        {
            _sfpUomMasterRepository = sfpUomMasterRepository;
        }
        public async Task<IEnumerable<SfpUomMaster>> GetUoms()
        {
            try
            {
                return await _sfpUomMasterRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpUomMaster> GetUom(int id)
        {
            try
            {
                var data = await _sfpUomMasterRepository.GetAsync(x => x.Id == id);
                if (data != null && data.Count() > 0)
                    return data.First();
                else
                    return new SfpUomMaster();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveUom(SfpUomMaster sfpUomMaster)
        {
            try
            {
                await _sfpUomMasterRepository.CreateAsync(sfpUomMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ClearData()
        {
            try
            {
                await _sfpUomMasterRepository.ClearData();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
