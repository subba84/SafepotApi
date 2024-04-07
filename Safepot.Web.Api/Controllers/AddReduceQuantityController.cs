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
    public class AddReduceQuantityController : ControllerBase
    {
        private readonly ISfpCustomizedQuantityService _sfpCustomizedQuantityService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ISfpUserService _sfpUserService;
        private readonly ISfpAgentCustDeliveryMapService _sfpAgentCustDeliveryMapService;
        private readonly INotificationService _notificationService;
        private readonly ISfpOrderService _sfpOrderService;

        private readonly ILogger<AddReduceQuantityController> _logger;
        public AddReduceQuantityController(ISfpCustomizedQuantityService sfpCustomizedQuantityService,
            ILogger<AddReduceQuantityController> logger,
            ISfpMakeModelMasterService sfpMakeModelMasterService,
            ISfpUserService sfpUserService,
            ISfpAgentCustDeliveryMapService sfpAgentCustDeliveryMapService,
            INotificationService notificationService,
            ISfpOrderService sfpOrderService)
        {
            _sfpCustomizedQuantityService = sfpCustomizedQuantityService;
            _logger = logger;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
            _sfpUserService = sfpUserService;
            _sfpAgentCustDeliveryMapService = sfpAgentCustDeliveryMapService;
            _notificationService = notificationService;
            _sfpOrderService = sfpOrderService;
        }

        [HttpGet]
        [Route("gettransactionsforcustomer")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> GetCustomizeQuantitiesforCustomer(int customerid,int agentid, int makeModelMasterId,DateTime fromDate,DateTime toDate)
        {
            try
            {
                var data = await _sfpCustomizedQuantityService.GetTransactionsbasedonCustomer(customerid, agentid, makeModelMasterId, fromDate, toDate);
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", (data.Count() > 0 ? "List Available" : "Data Not Available"), data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("savecustquantitydata")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> SaveCustomizeQuantity([FromBody] SfpCustomizeQuantity sfpCustomizeQuantity)
        {
            try
            {
                var makeModelData = await _sfpMakeModelMasterService.GetMakeModel(sfpCustomizeQuantity.MakeModelMasterId ?? 0);
                sfpCustomizeQuantity.MakeName = makeModelData.MakeName;
                sfpCustomizeQuantity.ModelName = makeModelData.ModelName;
                sfpCustomizeQuantity.UomName = makeModelData.UomName;
                sfpCustomizeQuantity.Price = makeModelData.Price;
                sfpCustomizeQuantity.UnitQuantity = Convert.ToString(makeModelData.Quantity);
                sfpCustomizeQuantity.UnitPrice = makeModelData.Price;
                sfpCustomizeQuantity.Status = "Pending";
                sfpCustomizeQuantity.TotalPrice = Convert.ToString(Convert.ToDouble(sfpCustomizeQuantity.UnitPrice ?? "0") * Convert.ToDouble(sfpCustomizeQuantity.Quantity ?? "0"));
                if (sfpCustomizeQuantity.CustomerId > 0)
                {
                    var customer = await _sfpUserService.GetUser(sfpCustomizeQuantity.CustomerId ?? 0);
                    sfpCustomizeQuantity.CustomerName = customer.FirstName + " " + customer.LastName;
                }
                if(sfpCustomizeQuantity.AgentId > 0)
                {
                    var agent = await _sfpUserService.GetUser(sfpCustomizeQuantity.AgentId ?? 0);
                    sfpCustomizeQuantity.AgentName = agent.FirstName + " " + agent.LastName;
                }
               
                sfpCustomizeQuantity.TransactionDate = (sfpCustomizeQuantity.TransactionDate == null ? sfpCustomizeQuantity.TransactionDate : sfpCustomizeQuantity.TransactionDate.Value.Date);

                //if (sfpCustomizeQuantity.TransactionDate == DateTime.Now.Date)
                //{

                //}

                var orderData = await _sfpOrderService.GetIndividualOrder(sfpCustomizeQuantity.CustomerId ?? 0, sfpCustomizeQuantity.AgentId ?? 0, sfpCustomizeQuantity.TransactionDate.Value, "Pending");
                if (orderData != null && orderData.Count() > 0)
                {
                    if (orderData.First().Status == "Pending")
                    {
                        foreach (var item in orderData)
                        {
                            if (item.MakeModelMasterId == sfpCustomizeQuantity.MakeModelMasterId)
                            {
                                item.Quantity = sfpCustomizeQuantity.Quantity;
                                item.Status = "Pending";
                                item.TotalPrice = Convert.ToString(Convert.ToDouble(sfpCustomizeQuantity.UnitPrice ?? "0") * Convert.ToDouble(sfpCustomizeQuantity.Quantity ?? "0"));
                                item.OrderModifiedOn = DateTime.Now;
                                await _sfpOrderService.UpdateOrder(item);
                            }
                            else
                            {
                                SfpOrder sfpOrder = new SfpOrder();
                                sfpOrder.AgentId = sfpCustomizeQuantity.AgentId;
                                sfpOrder.AgentName = sfpCustomizeQuantity.AgentName;
                                sfpOrder.CustomerId = sfpCustomizeQuantity.CustomerId;
                                sfpOrder.CustomerName = sfpCustomizeQuantity.CustomerName;
                                sfpOrder.TransactionDate = sfpCustomizeQuantity.TransactionDate;
                                sfpOrder.Status = "Pending";
                                sfpOrder.TotalPrice = sfpCustomizeQuantity.TotalPrice;
                                sfpOrder.MakeModelMasterId = sfpCustomizeQuantity.MakeModelMasterId;
                                sfpOrder.Quantity = sfpCustomizeQuantity.Quantity;
                                sfpOrder.UnitPrice = sfpCustomizeQuantity.UnitPrice;
                                item.OrderCreatedOn = DateTime.Now;
                                await _sfpOrderService.CreateOrder(sfpOrder);
                            }
                        }
                    }
                }

                string description = string.Empty;
                DateTime? today = DateTime.Now;
                if (sfpCustomizeQuantity.Id > 0)
                {
                    description = "New Order have been added/modified by " + sfpCustomizeQuantity.CustomerName + " on " + today;
                    await _sfpCustomizedQuantityService.UpdateCustomizedQuantity(sfpCustomizeQuantity);
                }
                else
                {
                    description = "Order have been added/modified by " + sfpCustomizeQuantity.CustomerName + " on " + today;
                    await _sfpCustomizedQuantityService.SaveCustomizedQuantity(sfpCustomizeQuantity);
                }
               
                await _notificationService.CreateNotification(description, sfpCustomizeQuantity.AgentId, sfpCustomizeQuantity.CustomerId,null, (sfpCustomizeQuantity.TransactionDate == null ? sfpCustomizeQuantity.TransactionDate : sfpCustomizeQuantity.TransactionDate.Value.Date),"Order Creation",true,true,true);
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", "Customer Quantity Save Successful", new List<SfpCustomizeQuantity>() { new SfpCustomizeQuantity() { Id = sfpCustomizeQuantity.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatecustquantitydata")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> UpdateCustomizeQuantity([FromBody] SfpCustomizeQuantity sfpCustomizeQuantity)
        {
            try
            {
                await _sfpCustomizedQuantityService.UpdateCustomizedQuantity(sfpCustomizeQuantity);
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", "Customer Quantity Update Successful", new List<SfpCustomizeQuantity>() { new SfpCustomizeQuantity() { Id = sfpCustomizeQuantity.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deletecustquantitydata")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> DeleteCustomerQuantityData(int customerid)
        {
            try
            {
                var data = await _sfpCustomizedQuantityService.GetAllTransactionsbasedonCustomer(customerid);
                if(data!=null && data.Count() > 0)
                {
                    foreach (var item in data.ToList())
                    {
                        await _sfpCustomizedQuantityService.DeleteCustomizedQuantity(item);
                    }
                }
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", "Customized Quantity Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
