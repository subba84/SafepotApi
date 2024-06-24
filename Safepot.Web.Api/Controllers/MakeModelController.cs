using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MakeModelController : ControllerBase
    {
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        public MakeModelController(ISfpMakeModelMasterService sfpMakeModelMasterService)
        {
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
        }

        [HttpGet]
        [Route("getmakemodels")]
        public async Task<ResponseModel<SfpMakeModelMaster>> GetAllMakeModels()
        {
            try
            {
                var data = await _sfpMakeModelMasterService.GetMakeModels();
                return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getmakemodels/{agentid}")]
        public async Task<ResponseModel<SfpMakeModelMaster>> GetAllMakeModelsbasedonAgent(int agentid)
        {
            try
            {
                var data = await _sfpMakeModelMasterService.GetMakeModelsbasedonAgent(agentid);
                return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("createmakemodel")]
        public async Task<ResponseModel<SfpMakeModelMaster>> CreateMakeModel([FromBody] SfpMakeModelMaster sfpMakeModelMaster)
        {
            try
            {
                var existedMakeModelData = await _sfpMakeModelMasterService.GetExistedMakeModels(sfpMakeModelMaster.AgentId,sfpMakeModelMaster.MakeId, sfpMakeModelMaster.ModelId, sfpMakeModelMaster.UomId, sfpMakeModelMaster.Quantity ?? 0);
                if(existedMakeModelData.Count() > 0)
                {
                    return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Duplicate", "Make Model Master Data Already Exists", new List<SfpMakeModelMaster>() { new SfpMakeModelMaster { Id = sfpMakeModelMaster.Id } });
                }
                await _sfpMakeModelMasterService.SaveMakeModel(sfpMakeModelMaster);
                return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Success", "Make Model Master Save Successful", new List<SfpMakeModelMaster>() { new SfpMakeModelMaster { Id= sfpMakeModelMaster.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatemakemodel")]
        public async Task<ResponseModel<SfpMakeModelMaster>> UpdateMakeModel([FromBody] SfpMakeModelMaster sfpMakeModelMaster)
        {
            try
            {
                await _sfpMakeModelMasterService.UpdateMakeModel(sfpMakeModelMaster);
                return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Success", "Make Model Master Update Successful", new List<SfpMakeModelMaster>() { new SfpMakeModelMaster { Id = sfpMakeModelMaster.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deletemakemodel")]
        public async Task<ResponseModel<SfpMakeModelMaster>> DeleteMakeModel(int id)
        {
            try
            {
                await _sfpMakeModelMasterService.DeleteMakeModel(id);
                return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Success", "Make Model Master Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMakeModelMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
