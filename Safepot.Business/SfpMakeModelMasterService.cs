using Microsoft.VisualBasic;
using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpMakeModelMasterService : ISfpMakeModelMasterService
    {
        private readonly ISfpDataRepository<SfpMakeModelMaster> _makeMasterRepository;

        public SfpMakeModelMasterService(ISfpDataRepository<SfpMakeModelMaster> makeMasterRepository)
        {
            _makeMasterRepository = makeMasterRepository;
        }
        public async Task DeleteMakeModel(int id)
        {
            try
            {
                var masterdata = await _makeMasterRepository.GetAsync(x => x.Id == id);
                if (masterdata != null && masterdata.Count() > 0)
                    await _makeMasterRepository.DeleteAsync(masterdata.First());
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpMakeModelMaster> GetMakeModel(int id)
        {
            try
            {
                var masterdata = await _makeMasterRepository.GetAsync(x => x.Id == id);
                if (masterdata != null && masterdata.Count() > 0)
                    return masterdata.First();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new SfpMakeModelMaster();
        }

        public async Task<IEnumerable<SfpMakeModelMaster>> GetMakeModels()
        {
            try
            {
                var masterdata = await _makeMasterRepository.GetAsync();
                if (masterdata != null && masterdata.Count() > 0)
                    return masterdata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpMakeModelMaster>();
        }

        public async Task<IEnumerable<SfpMakeModelMaster>> GetExistedMakeModels(int? agentId,int? makeId,int? modelId,int? uomId,decimal qty)
        {
            try
            {
                var masterdata = await _makeMasterRepository.GetAsync(x=>x.AgentId == agentId && x.MakeId == makeId && x.ModelId == modelId && x.UomId == uomId && x.Quantity == qty);
                if (masterdata != null && masterdata.Count() > 0)
                {
                    return masterdata;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpMakeModelMaster>();
        }

        public async Task<IEnumerable<SfpMakeModelMaster>> GetMakeModelsbasedonAgent(int agentId)
        {
            try
            {
                var masterdata = await _makeMasterRepository.GetAsync(x=>x.AgentId == agentId);
                if (masterdata != null && masterdata.Count() > 0)
                    return masterdata.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpMakeModelMaster>();
        }



        public async Task SaveMakeModel(SfpMakeModelMaster sfpMakeModelMaster)
        {
            try
            {
                await _makeMasterRepository.CreateAsync(sfpMakeModelMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateMakeModel(SfpMakeModelMaster sfpMakeModelMaster)
        {
            try
            {
                await _makeMasterRepository.UpdateAsync(sfpMakeModelMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
