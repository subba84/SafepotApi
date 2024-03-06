using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISfpSettingService _sfpSettingService;
        public SettingsController(ISfpSettingService sfpSettingService)
        {
            _sfpSettingService = sfpSettingService;
        }

        [HttpGet]
        [Route("getsettingforagent")]
        public async Task<ResponseModel<SfpSetting>> GetSettingforAgent(int agentId)
        {
            try
            {
                var data = await _sfpSettingService.GetSettingforAgent(agentId);
                return ResponseModel<SfpSetting>.ToApiResponse("Success", "List Available", new List<SfpSetting> { data });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpSetting>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("savesettingforagent")]
        public async Task<ResponseModel<SfpSetting>> SaveSettingforAgent(int agentId,string agentName,bool isDamageReturnAllowed)
        {
            try
            {
                SfpSetting sfpSetting = new SfpSetting();
                sfpSetting.AgentId = agentId;
                sfpSetting.AgentName = agentName;
                sfpSetting.IsDamageReturnAllowed = isDamageReturnAllowed;
                await _sfpSettingService.SaveSetting(sfpSetting);
                return ResponseModel<SfpSetting>.ToApiResponse("Success", "Agent Setting Saved Successful", new List<SfpSetting> { new SfpSetting { Id = sfpSetting.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpSetting>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
