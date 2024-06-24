using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryChargeController : ControllerBase
    {
        private readonly ISfpAgentCustDlivryChargeService _sfpAgentCustDlivryChargeService;
        private readonly ILogger<DeliveryChargeController> _logger;
        public DeliveryChargeController(ISfpAgentCustDlivryChargeService sfpAgentCustDlivryChargeService, ILogger<DeliveryChargeController> logger)
        {
            _sfpAgentCustDlivryChargeService = sfpAgentCustDlivryChargeService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getdeliverycharges")]
        public async Task<ResponseModel<SfpAgentCustDlivryCharge>> GetDeliveryCharges()
        {
            try
            {
                var data = await _sfpAgentCustDlivryChargeService.GetDeliveryCharges();
                return ResponseModel<SfpAgentCustDlivryCharge>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpAgentCustDlivryCharge>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getdeliverychargeforagentandcustomer")]
        public async Task<ResponseModel<SfpAgentCustDlivryCharge>> GetDeliveryChargebasedonAgentandCustomer(int agentid,int customerid)
        {
            try
            {
                var data = await _sfpAgentCustDlivryChargeService.GetDeliveryChargeforMonthbasedonAgentandCustomer(agentid, customerid);
                return ResponseModel<SfpAgentCustDlivryCharge>.ToApiResponse("Success", "List Available", new List<SfpAgentCustDlivryCharge> { data });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpAgentCustDlivryCharge>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }



        [HttpPost]
        [Route("savedeliverycharge")]
        public async Task<ResponseModel<SfpAgentCustDlivryCharge>> SaveDeliveryCharge([FromBody] SfpAgentCustDlivryCharge sfpAgentCustDlivryCharge)
        {
            try
            {
                await _sfpAgentCustDlivryChargeService.SaveAgentCustDeliveryCharge(sfpAgentCustDlivryCharge);
                return ResponseModel<SfpAgentCustDlivryCharge>.ToApiResponse("Success", "Delivery Charge Save Successful", new List<SfpAgentCustDlivryCharge>() { new SfpAgentCustDlivryCharge { Id = sfpAgentCustDlivryCharge .Id} });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpAgentCustDlivryCharge>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatedeliverycharge")]
        public async Task<ResponseModel<SfpAgentCustDlivryCharge>> UpdateDeliveryCharge([FromBody] SfpAgentCustDlivryCharge sfpAgentCustDlivryCharge)
        {
            try
            {
                await _sfpAgentCustDlivryChargeService.UpdateAgentCustDeliveryCharge(sfpAgentCustDlivryCharge);
                return ResponseModel<SfpAgentCustDlivryCharge>.ToApiResponse("Success", "Delivery Charge Update Successful", new List<SfpAgentCustDlivryCharge>() { new SfpAgentCustDlivryCharge { Id = sfpAgentCustDlivryCharge.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpAgentCustDlivryCharge>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deletedeliverycharge")]
        public async Task<ResponseModel<SfpAgentCustDlivryCharge>> DeleteDeliveryCharge(int id)
        {
            try
            {
                //var data = await _sfpAgentCustDlivryChargeService.GetDeliveryCharge(id);
                await _sfpAgentCustDlivryChargeService.DeleteAgentCustDeliveryCharge(id);
                return ResponseModel<SfpAgentCustDlivryCharge>.ToApiResponse("Success", "Delivery Charge Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpAgentCustDlivryCharge>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
