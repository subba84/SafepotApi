using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        //private readonly ISfpCustomizedQuantityService _sfpCustomizedQuantityService;
        private readonly ISfpOrderService _sfpOrderService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ISfpUserService _userService;
        private readonly ILogger<OrderDetailsController> _logger;
        public OrderDetailsController(ISfpOrderService sfpOrderService,
            ISfpUserService userService,
            ISfpMakeModelMasterService sfpMakeModelMasterService,
            ILogger<OrderDetailsController> logger)
        {
            _sfpOrderService = sfpOrderService;
            _userService=userService;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getorderdetails")]
        public async Task<ResponseModel<OrderDetailsModel>> GetOrderDetails(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                List<OrderDetailsModel> orders = new List<OrderDetailsModel>();
                if(status == "Pending" && (customerid > 0 || deliveryId > 0))
                {
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date;
                }
                var data = await _sfpOrderService.GetOrders(customerid, agentId, deliveryId, status, fromDate, toDate);
                if(data!=null && data.Count() > 0)
                {
                    var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModels();
                    var agentsData = await _userService.GetUsers(data.Select(x => x.AgentId).ToList());
                    var users = await _userService.GetUsers(data.Select(x=>x.CustomerId).ToList());
                    data.ToList().ForEach(x => x.TransactionDate = (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date));
                    var customers = from c in data
                                    group c by new
                                    {
                                        c.AgentId,
                                        //c.AgentName,
                                        c.TransactionDate,
                                        c.CustomerId,
                                        //c.CustomerName,
                                        c.Status
                                    } into gcs
                                    select new OrderDetailsModel()
                                    {
                                        AgentId = gcs.Key.AgentId,
                                        //AgentName = gcs.Key.AgentName,
                                        TransactionDate = gcs.Key.TransactionDate,
                                        CustomerId = gcs.Key.CustomerId,
                                        //CustomerName = gcs.Key.CustomerName,
                                        Status = gcs.Key.Status
                                    };

                    if(customers!=null && customers.Count() > 0)
                    {
                        foreach(var customer in customers.ToList())
                        {
                            OrderDetailsModel order = new OrderDetailsModel();
                            var customerData = users.Where(x => x.Id == customer.CustomerId).First();
                            order.CustomerId = customer.CustomerId;
                            order.CustomerName = customerData.FirstName + " " + customerData.LastName;
                            
                            order.TransactionDate = customer.TransactionDate;
                            order.Address = customerData.Address;
                            order.Mobile = customerData.Mobile;
                            order.AltMobile = customerData.AltMobile;
                            order.EmailId = customerData.EmailId;
                            order.StateName = customerData.StateName;
                            order.CityName = customerData.CityName;
                            order.LandMark = customerData.LandMark;
                            order.PinCode = customerData.PinCode;
                            order.Status = customer.Status;
                            if(customer.AgentId != null && customer.AgentId > 0)
                            {
                                var agentData = agentsData.First(x => x.Id == customer.AgentId);
                                order.AgentId = customer.AgentId;
                                order.AgentName = agentData.FirstName + " " + agentData.LastName;
                                order.AgentAddress = agentData.Address;
                                order.AgentMobile = agentData.Mobile;
                                order.AgentAltMobile = agentData.AltMobile;
                                order.AgentEmailId = agentData.EmailId;
                                order.AgentStateName = agentData.StateName;
                                order.AgentCityName = agentData.CityName;
                                order.AgentLandMark = agentData.LandMark;
                                order.AgentPinCode = agentData.PinCode;
                            }
                            


                            var consolidatedData = from c in data
                                                   where c.CustomerId == customer.CustomerId
                                                   where c.TransactionDate == customer.TransactionDate
                                                   group c by new
                                                   {
                                                       c.MakeModelMasterId
                                                   } into gcs
                                                   select new SfpCustomizeQuantity()
                                                   {
                                                       MakeModelMasterId = gcs.Key.MakeModelMasterId,
                                                       Quantity = Convert.ToString(gcs.Sum(x => Convert.ToInt32(x.Quantity))),
                                                       TotalPrice = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.TotalPrice)))
                                                   };
                            if(consolidatedData!=null && consolidatedData.Count() > 0)
                            {
                                var productMasterData = consolidatedData.ToList();
                                productMasterData.ForEach(x => {
                                    var makeModelData = makeModelMasterData.First(y => y.Id == x.MakeModelMasterId);
                                    x.MakeName = makeModelData.MakeName;
                                    x.ModelName = makeModelData.ModelName;
                                    x.UomName = makeModelData.UomName;
                                    x.UnitQuantity = Convert.ToString(makeModelData.Quantity);
                                    x.UnitPrice = makeModelData.Price;
                                });
                                order.Products = productMasterData;
                            }
                            orders.Add(order);
                        }
                    }
                    return ResponseModel<OrderDetailsModel>.ToApiResponse("Success", "List Available", orders);
                }
                return ResponseModel<OrderDetailsModel>.ToApiResponse("Success", "List Available", new List<OrderDetailsModel>());
            }
            catch (Exception ex)
            {
                return ResponseModel<OrderDetailsModel>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getsingleorder")]
        public async Task<ResponseModel<OrderDetailsModel>> GetSingleOrder(int customerid,int agentId, DateTime transactionDate,string status)
        {
            try
            {
                OrderDetailsModel order = new OrderDetailsModel();
                var data = await _sfpOrderService.GetIndividualOrder(customerid, agentId, transactionDate,status);
                if (data != null && data.Count() > 0)
                {
                    var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModels();
                    var agentsData = await _userService.GetUsers(data.Select(x => x.AgentId).ToList());
                    data.ToList().ForEach(x => x.TransactionDate = (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date));
                    var customers = from c in data
                                    group c by new
                                    {
                                        c.TransactionDate,
                                        c.CustomerId,
                                        c.CustomerName,
                                        c.AgentId,
                                        c.AgentName,
                                        c.Status
                                    } into gcs
                                    select new OrderDetailsModel()
                                    {
                                        TransactionDate = gcs.Key.TransactionDate,
                                        CustomerId = gcs.Key.CustomerId,
                                        CustomerName = gcs.Key.CustomerName,
                                        AgentId = gcs.Key.AgentId,
                                        AgentName = gcs.Key.AgentName,
                                        Status = gcs.Key.Status
                                    };

                    if (customers != null && customers.Count() > 0)
                    {
                        foreach (var customer in customers)
                        {
                            var user = await _userService.GetUser(customer.CustomerId ?? 0);
                            order.CustomerId = customer.CustomerId;
                            order.CustomerName = customer.CustomerName;
                            order.AgentId = customer.AgentId;
                            order.AgentName = customer.AgentName;
                            order.TransactionDate = customer.TransactionDate;
                            order.Address = user.Address;
                            order.PinCode = user.PinCode;
                            order.Mobile = user.Mobile;
                            order.EmailId = user.EmailId;
                            order.LandMark = user.LandMark;
                            order.StateName = user.StateName;
                            order.CityName = user.CityName;
                            order.Status = customer.Status;
                            if (customer.AgentId != null && customer.AgentId > 0)
                            {
                                var agentData = agentsData.First(x => x.Id == customer.AgentId);
                                order.AgentId = customer.AgentId;
                                order.AgentName = agentData.FirstName + " " + agentData.LastName;
                                order.AgentAddress = agentData.Address;
                                order.AgentMobile = agentData.Mobile;
                                order.AgentAltMobile = agentData.AltMobile;
                                order.AgentEmailId = agentData.EmailId;
                                order.AgentStateName = agentData.StateName;
                                order.AgentCityName = agentData.CityName;
                                order.AgentLandMark = agentData.LandMark;
                                order.AgentPinCode = agentData.PinCode;
                            }
                            var consolidatedData = from c in data
                                                   where c.CustomerId == customer.CustomerId
                                                   where c.TransactionDate == customer.TransactionDate
                                                   group c by new
                                                   {
                                                       c.MakeModelMasterId
                                                   } into gcs
                                                   select new SfpCustomizeQuantity()
                                                   {
                                                       MakeModelMasterId = gcs.Key.MakeModelMasterId,
                                                       Quantity = Convert.ToString(gcs.Sum(x => Convert.ToInt32(x.Quantity))),
                                                       TotalPrice = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.TotalPrice)))
                                                   };
                            if (consolidatedData != null && consolidatedData.Count() > 0)
                            {
                                var productMasterData = consolidatedData.ToList();
                                productMasterData.ForEach(x => {
                                    var makeModelData = makeModelMasterData.First(y => y.Id == x.MakeModelMasterId);
                                    x.MakeName = makeModelData.MakeName;
                                    x.ModelName = makeModelData.ModelName;
                                    x.UomName = makeModelData.UomName;
                                    x.UnitQuantity = Convert.ToString(makeModelData.Quantity);
                                    x.UnitPrice = makeModelData.Price;
                                });
                                order.Products = productMasterData;
                            }
                        }
                    }
                    return ResponseModel<OrderDetailsModel>.ToApiResponse("Success", "List Available", new List<OrderDetailsModel> { order });
                }
                return ResponseModel<OrderDetailsModel>.ToApiResponse("Success", "List Available", new List<OrderDetailsModel>());
            }
            catch (Exception ex)
            {
                return ResponseModel<OrderDetailsModel>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }


        [HttpPut]
        [Route("updateorderdetails")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> UpdateOrderDetails(int customerid, int agentId,int deliveryBoyId,string status)
        {
            try
            {
                string orderStatus = String.Empty;
                if (status == "Delivered")
                {
                    orderStatus = "Pending";
                }
                else if (status == "Completed")
                {
                    orderStatus = "Delivered";
                }
                else if (status == "Pending Rejected")
                {
                    orderStatus = "Pending";
                }
                else if (status == "Delivery Rejected")
                {
                    orderStatus = "Accepted";
                }
                var data = await _sfpOrderService.GetIndividualOrder(customerid, agentId, DateTime.Now.Date, orderStatus);
                if (data != null && data.Count() > 0)
                {
                    foreach (var item in data.ToList())
                    {
                        item.Status = status;
                        if (status == "Accepted")
                        {
                            item.OrderAcceptedBy = deliveryBoyId;
                            item.OrderAcceptedOn = DateTime.Now;
                        }
                        else if (status == "Completed")
                        {
                            item.OrderCompletedBy = deliveryBoyId;
                            item.OrderCompletedOn = DateTime.Now;
                        }
                        else if (status == "Rejected" || status == "Pending Rejected" || status == "Delivery Rejected")
                        {
                            item.Status = "Rejected";
                            item.OrderRejectedBy = deliveryBoyId;
                            item.OrderRejectedOn = DateTime.Now;
                        }
                        await _sfpOrderService.UpdateOrder(item);
                    }
                }
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", "Customer Quantity Update Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("createordersbasedonschedule")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> CreateOrdersbasedonSchedule()
        {
            try
            {
                await _sfpOrderService.CreateOrdersbasedonSchedule();
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", "Order Creation Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("rejectpastdatedpendingorders")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> RejectPastDatedPendingOrders()
        {
            try
            {
                await _sfpOrderService.RejectPastDatedPendingOrders();
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", "Past Dated Orders Rejection Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
