using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Mysqlx.Crud;
using NPOI.SS.Formula.Functions;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;
using System.Collections.Generic;
using System.Globalization;

namespace Safepot.Web.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly ISfpCustomerQuantityService _sfpCustomerQuantityService;
        private readonly ISfpOrderService _sfpOrderService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ISfpAgentCustDlivryChargeService _sfpDeliveryChargeService;
        private readonly ISfpAgentCustDeliveryMapService _sfpMappingService;
        private readonly ISfpUserService _userService;
        private readonly ISfpCustomerInvoiceService _customerInvoiceService;
        private readonly ISfpPaymentConfirmationService _paymentService;
        private readonly ILogger<OrderDetailsController> _logger;
        private readonly INotificationService _notificationService;
        public OrderDetailsController(ISfpOrderService sfpOrderService,
            ISfpUserService userService,
            ISfpMakeModelMasterService sfpMakeModelMasterService,
            ISfpAgentCustDlivryChargeService sfpDeliveryChargeService,
            ISfpAgentCustDeliveryMapService sfpMappingService,
            ISfpCustomerInvoiceService customerInvoiceService,
            ISfpPaymentConfirmationService paymentService,
            ISfpCustomerQuantityService sfpCustomerQuantityService,
            ILogger<OrderDetailsController> logger,
            INotificationService notificationService)
        {
            _sfpOrderService = sfpOrderService;
            _userService = userService;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
            _sfpDeliveryChargeService = sfpDeliveryChargeService;
            _sfpMappingService = sfpMappingService;
            _customerInvoiceService = customerInvoiceService;
            _paymentService = paymentService;
            _sfpCustomerQuantityService = sfpCustomerQuantityService;
            _logger = logger;
            _notificationService = notificationService;
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
                        // Update Status in Schedule(Customer Quantity)
                        if(item.Status == "Completed")
                        {
                            var scheduleDetails = await _sfpCustomerQuantityService.GetExistingCustomerQtybasedonDate(item.AgentId ?? 0,item.CustomerId,item.TransactionDate,item.TransactionDate,item.MakeModelMasterId);
                            if(scheduleDetails!=null && scheduleDetails.Count() > 0)
                            {
                                var schedule = scheduleDetails.First();
                                if(schedule.Status == "Approved" && schedule.DurationFlag != "Continuous" && (schedule.ToDate == null ? schedule.ToDate : schedule.ToDate.Value.Date) == (item.TransactionDate == null ? item.TransactionDate : item.TransactionDate.Value.Date) )
                                {
                                    schedule.Status = "Completed";
                                    await _sfpCustomerQuantityService.UpdateCustomerQuantity(schedule);
                                }
                            }
                        }

                        var makeModelData = await _sfpMakeModelMasterService.GetMakeModel(item.MakeModelMasterId ?? 0);
                        var deliveryBoy = new SfpUser();
                        if(deliveryBoyId > 0)
                        {
                            deliveryBoy = await _userService.GetUser(deliveryBoyId);
                        }
                        string description = "Customer - " + item.CustomerName +  "'s Order have been " + (item.Status == "Delivered" ? "accepted" : item.Status.ToLower()) + " by " + (deliveryBoy.Id > 0 ? deliveryBoy.FirstName + " " + deliveryBoy.LastName : "delivery boy ") 
                                + " for product - " + makeModelData.ModelName + "(" + makeModelData.MakeName + ")";
                        if (item.AgentId > 0 && item.CustomerId > 0)
                        {
                            var deliveryBoys = await _sfpMappingService.GetAssociatedDeliveryBoysbasedonAgentandCustomer(item.AgentId ?? 0, item.CustomerId ?? 0);
                            if (deliveryBoys != null && deliveryBoys.Count() > 0)
                            {
                                foreach (var delivery in deliveryBoys)
                                {
                                    await _notificationService.CreateNotification(description, item.AgentId, item.CustomerId, delivery.Id, (item.TransactionDate == null ? item.TransactionDate : item.TransactionDate.Value.Date), "Order Update", false, false, true);
                                }
                            }
                        }
                        await _notificationService.CreateNotification(description, item.AgentId, item.CustomerId, null, (item.TransactionDate == null ? item.TransactionDate : item.TransactionDate.Value.Date), "Order Update", true, true, false);
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
                                    var makeModelData = await _sfpMakeModelMasterService.GetMakeModel(ord.MakeModelMasterId ?? 0);
                                    var deliveryBoy = new SfpUser();
                                    if ((order.DeliveryBoyId ?? 0) > 0)
                                    {
                                        deliveryBoy = await _userService.GetUser(order.DeliveryBoyId ?? 0);
                                    }
                                    string description = "Order have been " + (order.Status ?? "") .ToLower() + " by " + (deliveryBoy.Id > 0 ? deliveryBoy.FirstName + " " + deliveryBoy.LastName : " delivery boy ")
                                            + " for product - " + makeModelData.ModelName + "(" + makeModelData.MakeName + ")";
                                    if (ord.AgentId > 0 && ord.CustomerId > 0)
                                    {
                                        var deliveryBoys = await _sfpMappingService.GetAssociatedDeliveryBoysbasedonAgentandCustomer(ord.AgentId ?? 0, ord.CustomerId ?? 0);
                                        if (deliveryBoys != null && deliveryBoys.Count() > 0)
                                        {
                                            foreach (var delivery in deliveryBoys)
                                            {
                                                await _notificationService.CreateNotification(description, ord.AgentId, ord.CustomerId, delivery.Id, (ord.TransactionDate == null ? ord.TransactionDate : ord.TransactionDate.Value.Date), "Order Update", false, false, true);
                                            }
                                        }
                                    }
                                    await _notificationService.CreateNotification(description, ord.AgentId, ord.CustomerId, null, (ord.TransactionDate == null ? ord.TransactionDate : ord.TransactionDate.Value.Date), "Order Update", true, true, false);
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
        public async Task<ResponseModel<MonthlyPrice>> CustomInvoiceReport(
           int agentId,
           int customerId)
        {
            double deliveryCharge = 0;
            try
            {
                List<MonthlyPrice> monthlyPrices = new List<MonthlyPrice>();
                //InvoiceReportModel model = new InvoiceReportModel();
                //model.MonthlyPrices = new List<MonthlyPrice>();
                var customer = await _userService.GetUser(customerId);
                //var agent = await _userService.GetUser(agentId);
                //model.AgentId = agent.Id;
                //model.AgentName = agent.FirstName + " " + agent.LastName;
                //model.CustomerId = customer.Id;
                //model.CustomerName = customer.FirstName + " " + customer.LastName;


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
                            monthlyPrice.InvoiceNumber = await _customerInvoiceService.GetCustomerInvoiceId(i.Year, i.Month, customerId);
                            monthlyPrices.Add(monthlyPrice);
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
                return ResponseModel<MonthlyPrice>.ToApiResponse("Success", "Data Available", monthlyPrices);
            }
            catch (Exception ex)
            {
                return ResponseModel<MonthlyPrice>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("monthlyinvoicereport")]
        public async Task<ResponseModel<MonthlyOrderModel>> MonthlyInvoiceReport(
           int agentId,
           int customerId,
           int invoiceId)
        {
            double deliveryCharge = 0;
            try
            {
                SfpCustomerInvoice sfpCustomerInvoice = await _customerInvoiceService.GetCustomerInvoice(invoiceId);
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
                model.InvoiceNumber = invoiceId;
                model.ShopBeside = "0";
                DateTime? fromDate = new DateTime(sfpCustomerInvoice.InvoiceYear, sfpCustomerInvoice.InvoiceMonth, 1);
                DateTime? toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                model.ValidFrom = fromDate.Value.ToString("dd.MM.yyyy");
                model.ValidTo = toDate.Value.ToString("dd.MM.yyyy");
                model.Products = new List<Product>();
                var orders = await _sfpOrderService.GetOrders(customerId, agentId, 0, "Completed", fromDate, toDate);
                if (orders != null && orders.Count() > 0)
                {
                    var monthOrderDetails = orders.Where(x => (x.TransactionDate == null ? 0 : x.TransactionDate.Value.Year) == sfpCustomerInvoice.InvoiceYear
                        && (x.TransactionDate == null ? 0 : x.TransactionDate.Value.Month) == sfpCustomerInvoice.InvoiceMonth);
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
                        if(ordersList!=null && ordersList.Count() > 0)
                        {
                            foreach(var order in ordersList)
                            {
                                Product product = new Product();
                                product.Title = order.ModelName;
                                product.Quantity = order.Quantity;
                                product.Amount = order.TotalPrice;
                                model.Products.Add(product);
                            }
                        }
                    }
                    
                    DateTime deliveryChargeStartDate = new DateTime(sfpCustomerInvoice.InvoiceYear, sfpCustomerInvoice.InvoiceMonth, 1);
                    DateTime deliveryChargeToDate = deliveryChargeStartDate.AddMonths(1).AddDays(-1);
                    if (fromDate.Value.Month == (customer.JoinDate == null ? 0 : customer.JoinDate.Value.Month))
                    {
                        deliveryChargeStartDate = (customer.JoinDate == null ? deliveryChargeStartDate : customer.JoinDate.Value.Date);
                    }
                    deliveryCharge = await _sfpDeliveryChargeService.GetDeliveryChargeforPeriodbasedonAgentandCustomer(agentId, customer.Id, deliveryChargeStartDate, deliveryChargeToDate);

                    // Delivery Charge
                    Product deliveryChargeproduct = new Product();
                    deliveryChargeproduct.Title = "Delivery Charge";
                    deliveryChargeproduct.Quantity = "0";
                    deliveryChargeproduct.Amount = deliveryCharge.ToString();
                    model.Products.Add(deliveryChargeproduct);

                    // Delivery Charge
                    //double advance = 0;
                    //var paymentHistory = await _paymentService.GetPaymentHistorybasedonAgentandCustomer(agentId, customerId);
                    //if(paymentHistory!=null && paymentHistory.Count() > 0)
                    //{
                    //    foreach(var history in paymentHistory)
                    //    {
                    //        advance += Convert.ToDouble(history.Amount ?? "0");
                    //    }
                    //}
                    //Product advanceProduct = new Product();
                    //advanceProduct.Title = "Advance";
                    //advanceProduct.Quantity = "0";
                    //advanceProduct.Amount = advance.ToString();
                    //model.Products.Add(advanceProduct);



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
