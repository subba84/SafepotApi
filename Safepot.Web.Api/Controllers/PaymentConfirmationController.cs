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
    public class PaymentConfirmationController : ControllerBase
    {
        private readonly ISfpPaymentConfirmationService _sfpPaymentConfirmationService;
        private readonly ISfpAgentCustDlivryChargeService _sfpAgentCustDlivryChargeService;
        private readonly ISfpOrderSwitchService _sfpOrderSwitchService;
        private readonly ILogger<PaymentConfirmationController> _logger;
        public PaymentConfirmationController(ISfpPaymentConfirmationService sfpPaymentConfirmationService, 
            ILogger<PaymentConfirmationController> logger,
            ISfpAgentCustDlivryChargeService sfpAgentCustDlivryChargeService, ISfpOrderSwitchService sfpOrderSwitchService)
        {
            _sfpPaymentConfirmationService = sfpPaymentConfirmationService;
            _sfpAgentCustDlivryChargeService = sfpAgentCustDlivryChargeService;
            _logger = logger;
            _sfpOrderSwitchService = sfpOrderSwitchService;
        }

        [HttpGet]
        [Route("getpaymentconfirmations")]
        public async Task<ResponseModel<SfpPaymentConfirmation>> GetPaymentConfirmations()
        {
            try
            {
                var data = await _sfpPaymentConfirmationService.GetPaymentConfirmations();
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getpaymentsbasedoncustomer")]
        public async Task<ResponseModel<SfpPaymentConfirmation>> GetPaymentsbasedonCustomer(int customerId,string status)
        {
            try
            {
                var data = await _sfpPaymentConfirmationService.GetPaymentConfirmationbyCustomer(customerId, status);
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getbalancebasedoncustomerandagent")]
        public async Task<ResponseModel<SfpPaymentConfirmation>> GetBalancebasedonCustomerandAgent(int customerId, int agentId)
        {
            try
            {
                var data = await _sfpPaymentConfirmationService.GetBalancebasedonCustomerandAgent(customerId, agentId);
                var deliveryCharge = await _sfpAgentCustDlivryChargeService.GetDeliveryChargeforAgentandCustomer(agentId, customerId);
                double totalBalance = data + deliveryCharge;
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Success", "List Available", new List<SfpPaymentConfirmation>() { new SfpPaymentConfirmation { BalanceAmount = Convert.ToString(totalBalance) } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }


        [HttpGet]
        [Route("getpaymentsforcustomersbasedondelivery")]
        public async Task<ResponseModel<SfpPaymentConfirmation>> GetPaymentsforCustomersbasedonDelivery(int deliveryId,string status)
        {
            try
            {
                var data = await _sfpPaymentConfirmationService.GetPaymentsforCustomersbasedonDelivery(deliveryId, status);
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getminimumbalancecustomers")]
        public async Task<ResponseModel<CustomerData>> GetMinimumBalanceCustomers(int agentid)
        {
            try
            {
                var data = await _sfpPaymentConfirmationService.GetMinimumBalanceCustomersbasedonAgent(agentid);
                List<CustomerData> customers = new List<CustomerData>();
                if(data!=null && data.Count() > 0)
                {
                    foreach(var customer in data)
                    {
                        if (!string.IsNullOrEmpty(customer.ApprovalStatus))
                        {
                            if(Convert.ToDouble(customer.ApprovalStatus) <= 100)
                            {
                                CustomerData customerData = new CustomerData();
                                var orderSwitchDetails = await _sfpOrderSwitchService.GetOrderSwitchDetails(agentid, customer.Id);
                                if (orderSwitchDetails!=null && orderSwitchDetails.Id > 0)
                                {
                                    customerData.OrderStatus = orderSwitchDetails.IsOrderGenerationOff == true ? "Deactive" : "Active";
                                    customerData.OrderOffDate = orderSwitchDetails.OrderGenerateOnOffFrom;
                                }
                                else
                                {
                                    customerData.OrderStatus = "Active";
                                    customerData.OrderOffDate = null;
                                }                                
                                customerData.Id = customer.Id;
                                customerData.CustomerName = customer.FirstName + " " + customer.LastName;
                                customerData.MobileNumber = customer.Mobile;
                                customerData.Balance = customer.ApprovalStatus;
                                customers.Add(customerData);
                            }
                        }
                    }
                }
                return ResponseModel<CustomerData>.ToApiResponse("Success", "List Available", customers);
            }
            catch (Exception ex)
            {
                return ResponseModel<CustomerData>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }


        [HttpGet]
        [Route("getpaymenthistorybasedonagentandcustomer")]
        public async Task<ResponseModel<SfpPaymentConfirmation>> GetPaymentHistorybasedonAgentandCustomer(int agentId, int customerId)
        {
            try
            {
                var balance = await _sfpPaymentConfirmationService.GetBalancebasedonCustomerandAgent(customerId, agentId);
                var data = await _sfpPaymentConfirmationService.GetPaymentHistorybasedonAgentandCustomer(agentId, customerId);
                var deliveryCharge = await _sfpAgentCustDlivryChargeService.GetDeliveryChargeforAgentandCustomer(agentId, customerId);
                string combinedString = balance + "~" + Math.Round(deliveryCharge,0);
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Success", combinedString, data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("savepaymentconfirmation")]
        public async Task<ResponseModel<SfpPaymentConfirmation>> SavePaymentConfirmation([FromBody] SfpPaymentConfirmation sfpPaymentConfirmation)
        {
            try
            {
                await _sfpPaymentConfirmationService.SavePaymentConfirmation(sfpPaymentConfirmation);                
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Success", "Payment Save Successful", new List<SfpPaymentConfirmation>() { new SfpPaymentConfirmation { Id= sfpPaymentConfirmation .Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatepaymentconfirmation")]
        public async Task<ResponseModel<SfpPaymentConfirmation>> UpdatePaymentConfirmation([FromBody] SfpPaymentConfirmation sfpPaymentConfirmation)
        {
            try
            {
                await _sfpPaymentConfirmationService.UpdatePaymentConfirmation(sfpPaymentConfirmation);
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Success", "Payment Update Successful", new List<SfpPaymentConfirmation>() { new SfpPaymentConfirmation { Id = sfpPaymentConfirmation.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deletepaymentconfirmation")]
        public async Task<ResponseModel<SfpPaymentConfirmation>> DeletePaymentConfirmation(int id)
        {
            try
            {
                var data = await _sfpPaymentConfirmationService.GetPaymentConfirmation(id);
                await _sfpPaymentConfirmationService.DeletePaymentConfirmation(data);
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Success", "Payment Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPaymentConfirmation>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
