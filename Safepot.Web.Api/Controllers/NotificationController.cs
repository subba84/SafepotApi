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
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        private readonly ILogger<NotificationController> _logger;
        public NotificationController(INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getnotifications")]
        public async Task<ResponseModel<Notification>> GetNotifications(int agentId,int customerId, int deliveryId)
        {
            try
            {
                var data = await _notificationService.GetNotifications(agentId, customerId, deliveryId);
                return ResponseModel<Notification>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<Notification>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatenotification")]
        public async Task<ResponseModel<Notification>> UpdateNotification([FromBody]Notification notification)
        {
            try
            {
                await _notificationService.UpdateNotification(notification);
                return ResponseModel<Notification>.ToApiResponse("Success", "Notification", new List<Notification> { new Notification { Id = notification .Id} });
            }
            catch (Exception ex)
            {
                return ResponseModel<Notification>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
