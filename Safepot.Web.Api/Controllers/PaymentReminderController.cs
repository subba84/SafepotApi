using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentReminderController : ControllerBase
    {
        private readonly ISfpPaymentReminderService _sfpPaymentReminderService;

        private readonly ILogger<PaymentReminderController> _logger;
        public PaymentReminderController(ISfpPaymentReminderService sfpPaymentReminderService,
            ILogger<PaymentReminderController> logger)
        {
            _sfpPaymentReminderService = sfpPaymentReminderService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getpaymentremindersforcustomer")]
        public async Task<ResponseModel<SfpPaymentReminder>> GetPaymentRemindersforCustomer(int customerid)
        {
            try
            {
                var data = await _sfpPaymentReminderService.GetReminders(customerid);
                return ResponseModel<SfpPaymentReminder>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentReminder>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getpaymentreminder")]
        public async Task<ResponseModel<SfpPaymentReminder>> GetPaymentReminder(int id)
        {
            try
            {
                var data = await _sfpPaymentReminderService.GetReminder(id);
                return ResponseModel<SfpPaymentReminder>.ToApiResponse("Success", "List Available", new List<SfpPaymentReminder>() { data });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentReminder>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("savepaymentreminder")]
        public async Task<ResponseModel<SfpPaymentReminder>> SaveCustomizeQuantity([FromBody] SfpPaymentReminder sfpPaymentReminder)
        {
            try
            {
                await _sfpPaymentReminderService.CreateAsync(sfpPaymentReminder);
                return ResponseModel<SfpPaymentReminder>.ToApiResponse("Success", "Payment Reminder Save Successful", new List<SfpPaymentReminder>() { new SfpPaymentReminder() { Id = sfpPaymentReminder.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentReminder>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
