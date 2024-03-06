using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipController : ControllerBase
    {
        private readonly ISfpSubscriptionHistoryService _sfpSubscriptionHistoryService;
        public MembershipController(ISfpSubscriptionHistoryService sfpSubscriptionHistoryService)
        {
            _sfpSubscriptionHistoryService = sfpSubscriptionHistoryService;
        }

        [HttpGet]
        [Route("getallmemberships/{agentid}")]
        public async Task<ResponseModel<SfpSubscriptionHistory>> GetAllMemberships(int agentid)
        {
            try
            {
                var data = await _sfpSubscriptionHistoryService.GetAllSubscriptionHistory(agentid);
                return ResponseModel<SfpSubscriptionHistory>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpSubscriptionHistory>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
        [HttpPost]
        [Route("createmembership")]
        public async Task<ResponseModel<SfpSubscriptionHistory>> CreateMembershipp([FromBody] SfpSubscriptionHistory sfpSubscriptionHistory)
        {
            try
            {
                if(sfpSubscriptionHistory.PlanStartDate!=null && sfpSubscriptionHistory.PlanEndDate != null)
                {
                    var duration = (Convert.ToDateTime(sfpSubscriptionHistory.PlanEndDate) - Convert.ToDateTime(sfpSubscriptionHistory.PlanStartDate)).TotalDays;
                    sfpSubscriptionHistory.Duration = duration + " Days";
                }
                await _sfpSubscriptionHistoryService.SaveSubscriptionHistory(sfpSubscriptionHistory);
                return ResponseModel<SfpSubscriptionHistory>.ToApiResponse("Success", "Membership Save Success", new List<SfpSubscriptionHistory>() { new SfpSubscriptionHistory { Id = sfpSubscriptionHistory.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpSubscriptionHistory>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatemembership")]
        public async Task<ResponseModel<SfpSubscriptionHistory>> UpdateMembership([FromBody] SfpSubscriptionHistory sfpSubscriptionHistory)
        {
            try
            {
                if (sfpSubscriptionHistory.PlanStartDate != null && sfpSubscriptionHistory.PlanEndDate != null)
                {
                    var duration = (Convert.ToDateTime(sfpSubscriptionHistory.PlanEndDate) - Convert.ToDateTime(sfpSubscriptionHistory.PlanStartDate)).TotalDays;
                    sfpSubscriptionHistory.Duration = duration + " Days";
                }
                await _sfpSubscriptionHistoryService.UpdateSubscriptionHistory(sfpSubscriptionHistory);
                return ResponseModel<SfpSubscriptionHistory>.ToApiResponse("Success", "Membership Update Success", new List<SfpSubscriptionHistory>() { new SfpSubscriptionHistory { Id = sfpSubscriptionHistory.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpSubscriptionHistory>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deletemembership")]
        public async Task<ResponseModel<SfpSubscriptionHistory>> DeleteMembership(int id)
        {
            try
            {
                await _sfpSubscriptionHistoryService.DeleteSubscriptionHistory(id);
                return ResponseModel<SfpSubscriptionHistory>.ToApiResponse("Success", "Membership Deletion Success", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpSubscriptionHistory>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
