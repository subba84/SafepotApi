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
    public class MembershipNotificationController : ControllerBase
    {
        private readonly ISfpMembershipNotificationService _sfpMembershipNotificationService;
        public MembershipNotificationController(ISfpMembershipNotificationService sfpMembershipNotificationService)
        {
            _sfpMembershipNotificationService = sfpMembershipNotificationService;
        }

        [HttpPost]
        [Route("createmembershipnotification")]
        public async Task<ResponseModel<SfpMembershipNotification>> CreateMembershipp([FromBody] SfpMembershipNotification sfpMembershipNotification)
        {
            try
            {
                await _sfpMembershipNotificationService.CreateMembershipNotification(sfpMembershipNotification);
                return ResponseModel<SfpMembershipNotification>.ToApiResponse("Success", "Membership Notification Save Success", new List<SfpMembershipNotification>() { new SfpMembershipNotification { Id = sfpMembershipNotification.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMembershipNotification>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
