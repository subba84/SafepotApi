using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISfpSettingService _sfpSettingService;
        private readonly ISfpOrderSwitchService _orderSwitchService;
        private readonly ISfpUserService _sfpUserService;
        private readonly INotificationService _notificationService;
        public SettingsController(ISfpSettingService sfpSettingService, 
            ISfpOrderSwitchService orderSwitchService,
            ISfpUserService sfpUserService,
            INotificationService notificationService)
        {
            _sfpSettingService = sfpSettingService;
            _orderSwitchService = orderSwitchService;
            _sfpUserService = sfpUserService;
            _notificationService = notificationService;
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

        [HttpPost]
        [Route("ordergenswitchforcustomer")]
        public async Task<ResponseModel<SfpOrderSwitch>> SaveOrderGenSwitchforCustomer(int agentId, int customerId,bool isOrderGenOff, DateTime? onOffDate)
        {
            try
            {
                SfpOrderSwitch sfpOrderSwitch = new SfpOrderSwitch();
                sfpOrderSwitch.AgentId = agentId;
                sfpOrderSwitch.CustomerId = customerId;
                sfpOrderSwitch.IsOrderGenerationOff = isOrderGenOff;
                sfpOrderSwitch.OrderGenerateOnOffFrom = onOffDate;
                await _orderSwitchService.SwitchOrderGeneration(sfpOrderSwitch);
                var agent = await _sfpUserService.GetUser(agentId);
                var customer = await _sfpUserService.GetUser(agentId);
                if(isOrderGenOff == true)
                {
                    string description = "Order Generation Service have been stopped for Customer - " + customer.FirstName + " " + customer.LastName + "  by Vendor - " + agent.FirstName + " " + agent.LastName + " on " + DateTime.Now.ToString("dd-MM-yyyy hh:mm");
                    //Notification creation
                    await _notificationService.CreateNotification(description, agentId, customerId, null, null, "Order Generation Status", true, true, false);
                    return ResponseModel<SfpOrderSwitch>.ToApiResponse("Success", "Order Generation Service Stopped Successfully", new List<SfpOrderSwitch> { new SfpOrderSwitch { Id = sfpOrderSwitch.Id } });                    
                }                    
                else
                {
                    string description = "Order Generation Service have been resumed for Customer - " + customer.FirstName + " " + customer.LastName + "  by Vendor - " + agent.FirstName + " " + agent.LastName + " on " + DateTime.Now.ToString("dd-MM-yyyy hh:mm");
                    //Notification creation
                    await _notificationService.CreateNotification(description, agentId, customerId, null, null, "Order Generation Status", true, true, false);
                    return ResponseModel<SfpOrderSwitch>.ToApiResponse("Success", "Order Generation Service Resumed Successfulyy", new List<SfpOrderSwitch> { new SfpOrderSwitch { Id = sfpOrderSwitch.Id } });
                }                    
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpOrderSwitch>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
