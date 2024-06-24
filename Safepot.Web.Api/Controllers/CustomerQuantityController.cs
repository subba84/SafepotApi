using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerQuantityController : ControllerBase
    {
        private readonly ISfpCustomerQuantityService _sfpCustomerQuantityService;
        private readonly ISfpCustomizedQuantityService _sfpCustomizedQuantityService;
        private readonly ILogger<CustomerQuantityController> _logger;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ISfpUserService _sfpUserService;
        private readonly ISfpOrderService _sfpOrderService;
        private readonly ISfpOrderSwitchService _sfpOrderSwitchService;
        public CustomerQuantityController(ISfpCustomerQuantityService sfpCustomerQuantityService,
            ILogger<CustomerQuantityController> logger,
            ISfpMakeModelMasterService sfpMakeModelMasterService,
            ISfpCustomizedQuantityService sfpCustomizedQuantityService,
            ISfpUserService sfpUserService,
            ISfpOrderService sfpOrderService,
            ISfpOrderSwitchService sfpOrderSwitchService)
        {
            _sfpCustomerQuantityService = sfpCustomerQuantityService;
            _logger = logger;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
            _sfpCustomizedQuantityService = sfpCustomizedQuantityService;
            _sfpUserService = sfpUserService;
            _sfpOrderService = sfpOrderService;
            _sfpOrderSwitchService = sfpOrderSwitchService;
        }

        [HttpGet]
        [Route("getcustquantitydata")]
        public async Task<ResponseModel<SfpCustomerQuantity>> GetCustomizeQuantitiesforCustomer()
        {
            try
            {
                var data = await _sfpCustomerQuantityService.GetQuantitiesforCustomer();
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }


        [HttpGet]
        [Route("getsegregatedproductsforcustomersbasedondelivery")]
        public async Task<ResponseModel<SfpCustomerQuantity>> GetCustomizeQuantitiesforCustomer(int deliveryBoyId)
        {
            try
            {
                var data = await _sfpCustomerQuantityService.GetSegregatedProductDataforCustomersbasedonDeliveryBoy(deliveryBoyId);
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getproductsbasedoncustomer/{customerid}")]
        public async Task<ResponseModel<SfpCustomerQuantity>> GetProductsbasedonCustomer(int customerid,int agentId,string status)
        {
            try
            {
                var data = await _sfpCustomerQuantityService.GetProductsbasedonCustomer(customerid, agentId, status);
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        //[HttpGet]
        //[Route("getordersbasedonagentandstatus/{agentid}")]
        //public async Task<ResponseModel<SfpCustomerQuantity>> GetPendingOrdersforAgent(int agentid)
        //{
        //    try
        //    {
        //        var data = await _sfpCustomerQuantityService.GetPendingOrdersforAgent(agentid);
        //        return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Success", "List Available", data);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
        //    }
        //}

        [HttpGet]
        [Route("getordersbasedoncustomerandstatus/{customerid}")]
        public async Task<ResponseModel<SfpCustomerQuantity>> GetOrdersbasedonCustomerandStatus(int customerid,string status)
        {
            try
            {
                var data = await _sfpCustomerQuantityService.GetOrdersforCustomerbasedonStatus(customerid, status);
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("savecustquantitydata")]
        public async Task<ResponseModel<SfpCustomerQuantity>> SaveCustomizeQuantity([FromBody] SfpCustomerQuantity sfpCustomerQuantity)
        {
            try
            {
                bool isOrderSchedulerneedtoRun = false;
                var existingData = await _sfpCustomizedQuantityService.GetExistingCustomizeQtybasedonDate(sfpCustomerQuantity.CustomerId, sfpCustomerQuantity.FromDate, sfpCustomerQuantity.ToDate, sfpCustomerQuantity.MakeModelId);
                if(existingData != null && existingData.Count() > 0)
                {
                    List<string> dates = existingData.Select(x => (x.TransactionDate == null ? "" : x.TransactionDate.Value.ToString("dd-MM-yyyy"))).Distinct().ToList();
                    return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Duplicate", "Already orders have been placed for " + string.Join(',', dates.Where(x=>!string.IsNullOrEmpty(x)).ToArray()), new List<SfpCustomerQuantity>() { new SfpCustomerQuantity { Id = sfpCustomerQuantity.Id } });
                }
                var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModel(sfpCustomerQuantity.MakeModelId == null ? 0 : Convert.ToInt32(sfpCustomerQuantity.MakeModelId));
                sfpCustomerQuantity.Price = makeModelMasterData.Price;
                sfpCustomerQuantity.MakeName = makeModelMasterData.MakeName;
                sfpCustomerQuantity.ModelName = makeModelMasterData.ModelName;
                sfpCustomerQuantity.UomName = makeModelMasterData.UomName;
                sfpCustomerQuantity.UnitQuantity = Convert.ToString(makeModelMasterData.Quantity);
                sfpCustomerQuantity.FromDate = (sfpCustomerQuantity.FromDate == null ? sfpCustomerQuantity.FromDate : sfpCustomerQuantity.FromDate.Value.Date);
                sfpCustomerQuantity.ToDate = (sfpCustomerQuantity.ToDate == null ? sfpCustomerQuantity.ToDate : sfpCustomerQuantity.ToDate.Value.Date);
                sfpCustomerQuantity.Status = "Pending";
                if (sfpCustomerQuantity.ToDate == null)
                {
                    sfpCustomerQuantity.ToDate = sfpCustomerQuantity.FromDate;
                }

                var customer = await _sfpUserService.GetUser(sfpCustomerQuantity.CustomerId ?? 0);
                if (customer != null && customer.Id > 0)
                {
                    if ((customer.IsMobileAppInstalled == null || customer.IsMobileAppInstalled == false) && sfpCustomerQuantity.OrderCreatedBy == "Agent")
                    {
                        sfpCustomerQuantity.Status = "Approved";
                        isOrderSchedulerneedtoRun = true;
                    }
                }
                await _sfpCustomerQuantityService.PerformApprovalAction(sfpCustomerQuantity);
                await _sfpCustomerQuantityService.SaveCustomerQuantity(sfpCustomerQuantity);

                var today = DateTime.Now.Date;
                if (isOrderSchedulerneedtoRun && (sfpCustomerQuantity.FromDate == null ? sfpCustomerQuantity.FromDate : sfpCustomerQuantity.FromDate.Value.Date) == today)
                {
                    bool isOrderGenerationOff = await _sfpOrderSwitchService.IsOrderGenerationOff(sfpCustomerQuantity.AgentId ?? 0, sfpCustomerQuantity.CustomerId ?? 0);
                    if(isOrderGenerationOff == false)
                    {
                        // Run the order scheduler to create the orders for today..
                        await _sfpOrderService.CreateOrdersbasedonSchedule();
                    }
                }

                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Success", "Schedule saved successfully", new List<SfpCustomerQuantity>() { new SfpCustomerQuantity { Id= sfpCustomerQuantity .Id} });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatecustquantitydata")]
        public async Task<ResponseModel<SfpCustomerQuantity>> UpdateCustomizeQuantity([FromBody] SfpCustomerQuantity sfpCustomerQuantity)
        {
            try
            {
                var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModel(sfpCustomerQuantity.MakeModelId == null ? 0 : Convert.ToInt32(sfpCustomerQuantity.MakeModelId));
                sfpCustomerQuantity.Price = makeModelMasterData.Price;
                sfpCustomerQuantity.MakeName = makeModelMasterData.MakeName;
                sfpCustomerQuantity.ModelName = makeModelMasterData.ModelName;
                sfpCustomerQuantity.UomName = makeModelMasterData.UomName;
                sfpCustomerQuantity.UnitQuantity = Convert.ToString(makeModelMasterData.Quantity);
                sfpCustomerQuantity.FromDate = (sfpCustomerQuantity.FromDate == null ? sfpCustomerQuantity.FromDate : sfpCustomerQuantity.FromDate.Value.Date);
                sfpCustomerQuantity.ToDate = (sfpCustomerQuantity.ToDate == null ? sfpCustomerQuantity.ToDate : sfpCustomerQuantity.ToDate.Value.Date);
                
                await _sfpCustomerQuantityService.UpdateCustomerQuantity(sfpCustomerQuantity);
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Success", "Order Update Successful", new List<SfpCustomerQuantity>() { new SfpCustomerQuantity { Id = sfpCustomerQuantity.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, new List<SfpCustomerQuantity>() { new SfpCustomerQuantity { Id = sfpCustomerQuantity.Id } });
            }
        }


        [HttpPost]
        [Route("orderapproval")]
        public async Task<ResponseModel<SfpCustomerQuantity>> OrderApproval([FromBody] SfpCustomerQuantity sfpCustomerQuantity)
        {
            try
            {
                bool isOrderSchedulerneedtoRun = false;
                var approvalData = await _sfpCustomerQuantityService.GetQuantityforCustomer(sfpCustomerQuantity.Id);
                approvalData.Status = sfpCustomerQuantity.Status;


                var today = DateTime.Now.Date;
                if(approvalData.Status == "Approved" && (approvalData.FromDate == null ? approvalData.FromDate : approvalData.FromDate.Value.Date) == today)
                {
                    isOrderSchedulerneedtoRun = true;
                }
                approvalData.ApprovedBy = sfpCustomerQuantity.ApprovedBy;
                await _sfpCustomerQuantityService.PerformApprovalAction(approvalData);
                if (isOrderSchedulerneedtoRun)
                {
                    // Run the order scheduler to create the orders for today..
                    await _sfpOrderService.CreateOrdersbasedonSchedule();
                }
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Success", ((sfpCustomerQuantity.Status == "Approved" || sfpCustomerQuantity.Status == "Partial Approved") ? "Schedule got approved Successful" : "Schedule got rejected"), new List<SfpCustomerQuantity>() { new SfpCustomerQuantity { Id = sfpCustomerQuantity.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, new List<SfpCustomerQuantity>() { new SfpCustomerQuantity { Id = sfpCustomerQuantity.Id } });
            }
        }

        [HttpDelete]
        [Route("deletecustquantitydata")]
        public async Task<ResponseModel<SfpCustomerQuantity>> DeleteCustomerQuantityData(int customerid)
        {
            try
            {
                var data = await _sfpCustomerQuantityService.GetQuantitiesforCustomer(customerid);
                await _sfpCustomerQuantityService.DeleteCustomerQuantity(data);
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Success", "Customer Quantity Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
