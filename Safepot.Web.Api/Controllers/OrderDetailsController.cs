using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;
using System.Globalization;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        //private readonly ISfpCustomizedQuantityService _sfpCustomizedQuantityService;
        private readonly ISfpOrderService _sfpOrderService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ISfpAgentCustDlivryChargeService _sfpDeliveryChargeService;
        private readonly ISfpAgentCustDeliveryMapService _sfpMappingService;
        private readonly ISfpUserService _userService;
        private readonly ILogger<OrderDetailsController> _logger;
        public OrderDetailsController(ISfpOrderService sfpOrderService,
            ISfpUserService userService,
            ISfpMakeModelMasterService sfpMakeModelMasterService,
            ISfpAgentCustDlivryChargeService sfpDeliveryChargeService,
            ISfpAgentCustDeliveryMapService sfpMappingService,
            ILogger<OrderDetailsController> logger)
        {
            _sfpOrderService = sfpOrderService;
            _userService=userService;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
            _sfpDeliveryChargeService = sfpDeliveryChargeService;
            _sfpMappingService = sfpMappingService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getorderdetails")]
        public async Task<ResponseModel<OrderDetailsModel>> GetOrderDetails(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                List<OrderDetailsModel> orders = new List<OrderDetailsModel>();
                if((status == "Pending" && (customerid > 0 || deliveryId > 0)))
                {
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date;
                }
                else if(status == "Completed")
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
                                        //c.AgentName,
                                        c.Status
                                    } into gcs
                                    select new OrderDetailsModel()
                                    {
                                        TransactionDate = gcs.Key.TransactionDate,
                                        CustomerId = gcs.Key.CustomerId,
                                        CustomerName = gcs.Key.CustomerName,
                                        AgentId = gcs.Key.AgentId,
                                        //AgentName = gcs.Key.AgentName,
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
                        item.OrderModifiedOn = DateTime.Now;
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

        [HttpGet]
        [Route("getordersforsync")]
        public async Task<ResponseModel<OrderDetailsModel>> GetOrdersforSync(DateTime? lastSyncDate,int deliveryBoyId)
        {
            try
            {
                List<OrderDetailsModel> orders = new List<OrderDetailsModel>();
                var data = await _sfpOrderService.GetOrdersforSync(lastSyncDate, deliveryBoyId);
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
                                        c.OrderCode,
                                        c.Status
                                    } into gcs
                                    select new OrderDetailsModel()
                                    {
                                        TransactionDate = gcs.Key.TransactionDate,
                                        CustomerId = gcs.Key.CustomerId,
                                        CustomerName = gcs.Key.CustomerName,
                                        AgentId = gcs.Key.AgentId,
                                        OrderCode = gcs.Key.OrderCode,
                                        Status = gcs.Key.Status
                                    };

                    if (customers != null && customers.Count() > 0)
                    {
                        foreach (var customer in customers)
                        {
                            OrderDetailsModel order = new OrderDetailsModel();
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
                            order.OrderCode = customer.OrderCode;
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
                            //order.SyncDateTime = DateTime.Now;
                            orders.Add(order);
                        }
                    }
                    return ResponseModel<OrderDetailsModel>.ToApiResponse("Success", DateTime.Now.ToString(), orders);
                }
                return ResponseModel<OrderDetailsModel>.ToApiResponse("Success", "List Available", new List<OrderDetailsModel>());
            }
            catch (Exception ex)
            {
                return ResponseModel<OrderDetailsModel>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("syncorders")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> SyncOrders(List<SyncModel> orders)
        {
            try
            {
                if(orders!=null && orders.Count() > 0)
                {
                    foreach(var order in orders)
                    {
                        var serverOrderDetails = await _sfpOrderService.GetOrders(0,
                            0,
                            order.DeliveryBoyId ?? 0,
                            String.Empty,
                            null,
                            null
                            );
                        if(serverOrderDetails != null && serverOrderDetails.Count() > 0)
                        {
                            serverOrderDetails = serverOrderDetails.Where(x => x.OrderCode == order.OrderCode);
                            if (serverOrderDetails != null && serverOrderDetails.Count() > 0)
                            {
                                foreach (var ord in serverOrderDetails.ToList())
                                {
                                    ord.Status = order.Status;
                                    if (order.Status == "Accepted")
                                    {
                                        ord.OrderAcceptedBy = order.DeliveryBoyId;
                                        ord.OrderAcceptedOn = DateTime.Now;
                                    }
                                    else if (order.Status == "Completed")
                                    {
                                        ord.OrderCompletedBy = order.DeliveryBoyId;
                                        ord.OrderCompletedOn = DateTime.Now;
                                    }
                                    else if (order.Status == "Rejected")
                                    {
                                        ord.OrderRejectedBy = order.DeliveryBoyId;
                                        ord.OrderRejectedOn = DateTime.Now;
                                    }
                                    await _sfpOrderService.UpdateOrder(ord);
                                }
                            }
                        }
                    }
                }
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", "Order Sync Successfully", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("itemwisebillingreport")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> ItemwiseBillingReport(
           int agentId,
           int customerId,
           DateTime fromDate,
           DateTime toDate,
           int makeModelMasterId)
        {
            double deliveryCharge = 0;
            var ordersList = new List<SfpCustomizeQuantity>();
            try
            {
                var orders = await _sfpOrderService.GetOrders(customerId, agentId, 0, "Completed", fromDate, toDate);
                if (orders != null && orders.Count() > 0)
                {
                    var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModels();
                    if (customerId > 0)
                    {
                        orders = orders.Where(x => x.CustomerId == customerId);
                    }
                    if (makeModelMasterId > 0)
                    {
                        orders = orders.Where(x => x.MakeModelMasterId == makeModelMasterId);
                    }
                    if (orders != null && orders.Count() > 0)
                    {
                        var groupedOrders = from c in orders
                                            group c by new
                                            {
                                                c.TransactionDate,
                                                c.MakeModelMasterId
                                            } into gcs
                                            select new SfpCustomizeQuantity()
                                            {
                                                TransactionDate = gcs.Key.TransactionDate,
                                                MakeModelMasterId = gcs.Key.MakeModelMasterId,
                                                Quantity = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.Quantity))),
                                                TotalPrice = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.TotalPrice))),
                                            };
                        ordersList = groupedOrders.ToList();
                        ordersList.ForEach(x => {
                            var makeModelData = makeModelMasterData.First(y => y.Id == x.MakeModelMasterId);
                            x.MakeName = makeModelData.MakeName;
                            x.ModelName = makeModelData.ModelName;
                            x.UomName = makeModelData.UomName;
                            x.UnitQuantity = Convert.ToString(makeModelData.Quantity);
                            x.UnitPrice = makeModelData.Price;
                        });
                    }

                    
                    if(agentId > 0)
                    {
                        if(customerId > 0)
                        {
                            deliveryCharge += await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customerId, fromDate, toDate);
                        }
                        else
                        {
                            var customers = await _sfpMappingService.GetAgentAssociatedCustomers(agentId);
                            if(customers!=null && customers.Count() > 0)
                            {
                                foreach(var customer in customers)
                                {
                                    deliveryCharge += await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customer.Id, fromDate, toDate);
                                }
                            }
                        }
                    }
                }

                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", Convert.ToString(deliveryCharge), ordersList);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
            
        }

        [HttpGet]
        [Route("invoicereport")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> InvoiceReport(
           int agentId,
           int customerId,
           DateTime fromDate,
           DateTime toDate,
           int makeModelMasterId)
        {
            double deliveryCharge = 0;
            var ordersList = new List<SfpCustomizeQuantity>();
            try
            {
                var orders = await _sfpOrderService.GetOrders(customerId, agentId, 0, "Completed", fromDate, toDate);
                if (orders != null && orders.Count() > 0)
                {
                    var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModels();
                    if (customerId > 0)
                    {
                        orders = orders.Where(x => x.CustomerId == customerId);
                    }
                    if (makeModelMasterId > 0)
                    {
                        orders = orders.Where(x => x.MakeModelMasterId == makeModelMasterId);
                    }
                    if (orders != null && orders.Count() > 0)
                    {
                        var groupedOrders = from c in orders
                                            group c by new
                                            {
                                                c.MakeModelMasterId
                                            } into gcs
                                            select new SfpCustomizeQuantity()
                                            {
                                                MakeModelMasterId = gcs.Key.MakeModelMasterId,
                                                Quantity = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.Quantity))),
                                                TotalPrice = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.TotalPrice))),
                                            };
                        ordersList = groupedOrders.ToList();
                        ordersList.ForEach(x => {
                            var makeModelData = makeModelMasterData.First(y => y.Id == x.MakeModelMasterId);
                            x.MakeName = makeModelData.MakeName;
                            x.ModelName = makeModelData.ModelName;
                            x.UomName = makeModelData.UomName;
                            x.UnitQuantity = Convert.ToString(makeModelData.Quantity);
                            x.UnitPrice = makeModelData.Price;
                        });
                    }

                    if (agentId > 0)
                    {
                        if (customerId > 0)
                        {
                            deliveryCharge += await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customerId, fromDate, toDate);
                        }
                        else
                        {
                            var customers = await _sfpMappingService.GetAgentAssociatedCustomers(agentId);
                            if (customers != null && customers.Count() > 0)
                            {
                                foreach (var customer in customers)
                                {
                                    deliveryCharge += await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customer.Id, fromDate, toDate);
                                }
                            }
                        }
                    }
                }
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", Convert.ToString(deliveryCharge), ordersList);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("custominvoicereport")]
        public async Task<ResponseModel<InvoiceReportModel>> CustomInvoiceReport(
           int agentId,
           int customerId)
        {
            double deliveryCharge = 0;
            try
            {
                InvoiceReportModel model = new InvoiceReportModel();
                model.MonthlyPrices = new List<MonthlyPrice>();
                var customer = await _userService.GetUser(customerId);
                var agent = await _userService.GetUser(agentId);
                model.AgentId = agent.Id;
                model.AgentName = agent.FirstName + " " + agent.LastName;
                model.CustomerId = customer.Id;
                model.CustomerName = customer.FirstName + " " + customer.LastName;


                DateTime? fromDate = customer.JoinDate;
                DateTime? toDate = DateTime.Now.Date;
                var orders = await _sfpOrderService.GetOrders(customerId, agentId, 0, "Completed", fromDate, toDate);
                // Month Calculation between two dates
                DateTime fromMonthStartDate = fromDate == null ? DateTime.MinValue : new DateTime(fromDate.Value.Year, fromDate.Value.Month, 1);
                DateTime toMonthStartDate = toDate == null ? DateTime.MinValue : new DateTime(toDate.Value.Year, toDate.Value.Month, 1);
                if (orders != null && orders.Count() > 0)
                {
                    for (DateTime i = fromMonthStartDate; i <= toMonthStartDate; i = i.AddMonths(1))
                    {
                        var monthOrderDetails = orders.Where(x => (x.TransactionDate == null ? 0 : x.TransactionDate.Value.Year) == i.Year
                        && (x.TransactionDate == null ? 0 : x.TransactionDate.Value.Month) == i.Month);
                        if(monthOrderDetails!=null && monthOrderDetails.Count() > 0)
                        {
                            MonthlyPrice monthlyPrice = new MonthlyPrice();
                            monthlyPrice.Year = i.Year;
                            monthlyPrice.Month = i.Month;
                            monthlyPrice.MonthName = i.Year + " - " + i.ToString("MMMM", CultureInfo.InvariantCulture);
                            monthlyPrice.TotalAmount = monthOrderDetails.Sum(x => Convert.ToDouble(x.TotalPrice ?? "0"));
                            DateTime deliveryChargeStartDate = new DateTime(i.Year, i.Month, 1);
                            DateTime deliveryChargeToDate = deliveryChargeStartDate.AddMonths(1).AddDays(-1);
                            if (i.Month == (fromDate == null ? 0 : fromDate.Value.Month))
                            {
                                deliveryChargeStartDate = (fromDate == null ? deliveryChargeStartDate : fromDate.Value.Date);
                            }
                            deliveryCharge = await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customer.Id, deliveryChargeStartDate, deliveryChargeToDate);
                            monthlyPrice.TotalAmount = monthlyPrice.TotalAmount + deliveryCharge;
                            model.MonthlyPrices.Add(monthlyPrice);
                        }
                    }




                    //var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModels();
                    //if (customerId > 0)
                    //{
                    //    orders = orders.Where(x => x.CustomerId == customerId);
                    //}                    
                    //if (orders != null && orders.Count() > 0)
                    //{
                    //    var groupedOrders = from c in orders
                    //                        group c by new
                    //                        {
                    //                            c.MakeModelMasterId
                    //                        } into gcs
                    //                        select new SfpCustomizeQuantity()
                    //                        {
                    //                            MakeModelMasterId = gcs.Key.MakeModelMasterId,
                    //                            Quantity = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.Quantity))),
                    //                            TotalPrice = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.TotalPrice))),
                    //                        };
                    //    ordersList = groupedOrders.ToList();
                    //    ordersList.ForEach(x => {
                    //        var makeModelData = makeModelMasterData.First(y => y.Id == x.MakeModelMasterId);
                    //        x.MakeName = makeModelData.MakeName;
                    //        x.ModelName = makeModelData.ModelName;
                    //        x.UomName = makeModelData.UomName;
                    //        x.UnitQuantity = Convert.ToString(makeModelData.Quantity);
                    //        x.UnitPrice = makeModelData.Price;
                    //    });
                    //}

                    //if (agentId > 0)
                    //{
                    //    if (customerId > 0)
                    //    {
                    //        deliveryCharge += await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customerId, fromDate, toDate);
                    //    }
                    //    else
                    //    {
                    //        var customers = await _sfpMappingService.GetAgentAssociatedCustomers(agentId);
                    //        if (customers != null && customers.Count() > 0)
                    //        {
                    //            foreach (var customer in customers)
                    //            {
                    //                deliveryCharge += await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customer.Id, fromDate, toDate);
                    //            }
                    //        }
                    //    }
                    //}
                }
                return ResponseModel<InvoiceReportModel>.ToApiResponse("Success", "Data Available", new List<InvoiceReportModel> { model });
            }
            catch (Exception ex)
            {
                return ResponseModel<InvoiceReportModel>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("monthlyinvoicereport")]
        public async Task<ResponseModel<MonthlyOrderModel>> MonthlyInvoiceReport(
           int agentId,
           int customerId,
           int year,
           int month)
        {
            double deliveryCharge = 0;
            try
            {
                MonthlyOrderModel model = new MonthlyOrderModel();
                var customer = await _userService.GetUser(customerId);
                var agent = await _userService.GetUser(agentId);
                model.AgentId = agent.Id;
                model.AgentName = agent.FirstName + " " + agent.LastName;
                model.AgentMobileNumber = agent.Mobile;
                model.AgentAddress = agent.Address;
                model.AgentLandmark = agent.LandMark;
                model.AgentState = agent.StateName;
                model.AgentCity = agent.CityName;
                model.AgentPincode = agent.PinCode;
                model.CustomerId = customer.Id;
                model.CustomerName = customer.FirstName + " " + customer.LastName;
                DateTime? fromDate = new DateTime(year,month,1);
                DateTime? toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                model.ValidFrom = fromDate.Value.ToString("dd-MMMM-yyyy");
                model.ValidTo = toDate.Value.ToString("dd-MMMM-yyyy");
                model.Products = new List<SfpCustomizeQuantity>();
                var orders = await _sfpOrderService.GetOrders(customerId, agentId, 0, "Completed", fromDate, toDate);
                if (orders != null && orders.Count() > 0)
                {
                    var monthOrderDetails = orders.Where(x => (x.TransactionDate == null ? 0 : x.TransactionDate.Value.Year) == year
                        && (x.TransactionDate == null ? 0 : x.TransactionDate.Value.Month) == month);
                    if (monthOrderDetails != null && monthOrderDetails.Count() > 0)
                    {
                        var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModels();
                        var groupedOrders = from c in monthOrderDetails
                                            group c by new
                                            {
                                                c.MakeModelMasterId
                                            } into gcs
                                            select new SfpCustomizeQuantity()
                                            {
                                                MakeModelMasterId = gcs.Key.MakeModelMasterId,
                                                Quantity = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.Quantity))),
                                                TotalPrice = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.TotalPrice))),
                                            };
                        var ordersList = groupedOrders.ToList();
                        ordersList.ForEach(x =>
                        {
                            var makeModelData = makeModelMasterData.First(y => y.Id == x.MakeModelMasterId);
                            x.MakeName = makeModelData.MakeName;
                            x.ModelName = makeModelData.ModelName;
                            x.UomName = makeModelData.UomName;
                            x.UnitQuantity = Convert.ToString(makeModelData.Quantity);
                            x.UnitPrice = makeModelData.Price;
                        });
                        model.Products = ordersList;
                        model.TotalAmount = monthOrderDetails.Sum(x => Convert.ToDouble(x.TotalPrice ?? "0"));
                        DateTime deliveryChargeStartDate = new DateTime(year, month, 1);
                        DateTime deliveryChargeToDate = deliveryChargeStartDate.AddMonths(1).AddDays(-1);
                        if (fromDate.Value.Month == (customer.JoinDate == null ? 0 : customer.JoinDate.Value.Month))
                        {
                            deliveryChargeStartDate = (customer.JoinDate == null ? deliveryChargeStartDate : customer.JoinDate.Value.Date);
                        }
                        deliveryCharge = await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customer.Id, deliveryChargeStartDate, deliveryChargeToDate);
                        model.DeliveryCharge = deliveryCharge;
                        model.TotalAmount = model.TotalAmount + deliveryCharge;
                    }




                    //var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModels();
                    //if (customerId > 0)
                    //{
                    //    orders = orders.Where(x => x.CustomerId == customerId);
                    //}                    
                    //if (orders != null && orders.Count() > 0)
                    //{
                    //    var groupedOrders = from c in orders
                    //                        group c by new
                    //                        {
                    //                            c.MakeModelMasterId
                    //                        } into gcs
                    //                        select new SfpCustomizeQuantity()
                    //                        {
                    //                            MakeModelMasterId = gcs.Key.MakeModelMasterId,
                    //                            Quantity = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.Quantity))),
                    //                            TotalPrice = Convert.ToString(gcs.Sum(x => Convert.ToDouble(x.TotalPrice))),
                    //                        };
                    //    ordersList = groupedOrders.ToList();
                    //    ordersList.ForEach(x => {
                    //        var makeModelData = makeModelMasterData.First(y => y.Id == x.MakeModelMasterId);
                    //        x.MakeName = makeModelData.MakeName;
                    //        x.ModelName = makeModelData.ModelName;
                    //        x.UomName = makeModelData.UomName;
                    //        x.UnitQuantity = Convert.ToString(makeModelData.Quantity);
                    //        x.UnitPrice = makeModelData.Price;
                    //    });
                    //}

                    //if (agentId > 0)
                    //{
                    //    if (customerId > 0)
                    //    {
                    //        deliveryCharge += await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customerId, fromDate, toDate);
                    //    }
                    //    else
                    //    {
                    //        var customers = await _sfpMappingService.GetAgentAssociatedCustomers(agentId);
                    //        if (customers != null && customers.Count() > 0)
                    //        {
                    //            foreach (var customer in customers)
                    //            {
                    //                deliveryCharge += await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customer.Id, fromDate, toDate);
                    //            }
                    //        }
                    //    }
                    //}
                }
                return ResponseModel<MonthlyOrderModel>.ToApiResponse("Success", "Data Available", new List<MonthlyOrderModel> { model });
            }
            catch (Exception ex)
            {
                return ResponseModel<MonthlyOrderModel>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
