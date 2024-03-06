using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CutoffTimeController : ControllerBase
    {
        private readonly ISfpCutoffTimeMasterService _sfpCutoffTimeMasterService;
        private readonly ILogger<CutoffTimeController> _logger;
        public CutoffTimeController(ISfpCutoffTimeMasterService sfpCutoffTimeMasterService, ILogger<CutoffTimeController> logger)
        {
            _sfpCutoffTimeMasterService = sfpCutoffTimeMasterService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getcutifftime")]
        public async Task<ResponseModel<SfpCutoffTimeMaster>> GetCutoffTimeData()
        {
            try
            {
                var data = await _sfpCutoffTimeMasterService.GetSfpCutoffTimeMasters();
                return ResponseModel<SfpCutoffTimeMaster>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCutoffTimeMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getcutifftimebasedonagent/{agentid}")]
        public async Task<ResponseModel<SfpCutoffTimeMaster>> GetCutoffTimeDatabasedonAgent(int agentid)
        {
            try
            {
                var data = await _sfpCutoffTimeMasterService.GetCutoffTimebasedonAgent(agentid);
                return ResponseModel<SfpCutoffTimeMaster>.ToApiResponse("Success", "List Available", new List<SfpCutoffTimeMaster> { data });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCutoffTimeMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }



        [HttpPost]
        [Route("savecutofftimedata")]
        public async Task<ResponseModel<SfpCutoffTimeMaster>> SaveCutoffTimeData([FromBody] SfpCutoffTimeMaster sfpCutoffTimeMaster)
        {
            try
            {
                await _sfpCutoffTimeMasterService.CreateCutoffTimeData(sfpCutoffTimeMaster);
                return ResponseModel<SfpCutoffTimeMaster>.ToApiResponse("Success", "Cutoff Time Save Successful", new List<SfpCutoffTimeMaster>() { new SfpCutoffTimeMaster { Id = sfpCutoffTimeMaster.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCutoffTimeMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatecutofftimedata")]
        public async Task<ResponseModel<SfpCutoffTimeMaster>> UpdateCutoffTimeData([FromBody] SfpCutoffTimeMaster sfpCutoffTimeMaster)
        {
            try
            {
                await _sfpCutoffTimeMasterService.UpdateCutoffTimeData(sfpCutoffTimeMaster);
                return ResponseModel<SfpCutoffTimeMaster>.ToApiResponse("Success", "Cutoff Time Update Successful", new List<SfpCutoffTimeMaster>() { new SfpCutoffTimeMaster { Id = sfpCutoffTimeMaster.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCutoffTimeMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deletecutofftime")]
        public async Task<ResponseModel<SfpCutoffTimeMaster>> DeleteCutoffTime(int id)
        {
            try
            {
                await _sfpCutoffTimeMasterService.DeleteCutoffTimeData(id);
                return ResponseModel<SfpCutoffTimeMaster>.ToApiResponse("Success", "Cutoff Time Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCutoffTimeMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
