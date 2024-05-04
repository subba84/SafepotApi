using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpOrderService : ISfpOrderService
    {
        private readonly ISfpDataRepository<SfpOrder> _sfpOrderRepository;
        private readonly ISfpCustomerQuantityService _sfpCustomerQuantityService;
        private readonly ISfpCustomizedQuantityService _sfpCustomizeQuantityService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ISfpCustomerAbsentService _sfpCustomerAbsentService;
        private readonly ISfpAgentCustDeliveryMapService _agentCustDeliveryMapService;
        private readonly INotificationService _notificationService;

        public SfpOrderService(ISfpDataRepository<SfpOrder> sfpOrderRepository,
            ISfpAgentCustDeliveryMapService agentCustDeliveryMapService,
            ISfpMakeModelMasterService sfpMakeModelMasterService,
            ISfpCustomerAbsentService sfpCustomerAbsentService,
            ISfpCustomerQuantityService sfpCustomerQuantityService,
            ISfpCustomizedQuantityService sfpCustomizeQuantityService,
            INotificationService notificationService)
        {
            _sfpOrderRepository = sfpOrderRepository;
            _agentCustDeliveryMapService = agentCustDeliveryMapService;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
            _sfpCustomerAbsentService = sfpCustomerAbsentService;
            _sfpCustomerQuantityService = sfpCustomerQuantityService;
            _sfpCustomizeQuantityService = sfpCustomizeQuantityService;
            _notificationService = notificationService;
        }

        public async Task CreateOrder(SfpOrder sfpOrder)
        {
            try
            {
                var existingData = await _sfpOrderRepository.GetAsync(x => x.AgentId == sfpOrder.AgentId
                && x.CustomerId == sfpOrder.CustomerId
                && x.MakeModelMasterId == sfpOrder.MakeModelMasterId
                && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == DateTime.Now.Date);
                if(existingData == null || existingData.Count() == 0)
                {
                    Guid guid = Guid.NewGuid();
                    sfpOrder.OrderCode = guid.ToString();

                    var existingOrderCodeDetails = await _sfpOrderRepository.GetAsync(x => x.AgentId == sfpOrder.AgentId
                && x.CustomerId == sfpOrder.CustomerId
                && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == DateTime.Now.Date);
                    if( existingOrderCodeDetails != null && existingOrderCodeDetails.Count() > 0)
                    {
                        if(sfpOrder.Status == existingOrderCodeDetails.First().Status)
                        {
                            sfpOrder.OrderCode = existingOrderCodeDetails.First().OrderCode;
                        }
                    }

                    await _sfpOrderRepository.CreateAsync(sfpOrder);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateOrder(SfpOrder sfpOrder)
        {
            try
            {
                string? orderStatus = sfpOrder.Status;
                var existingOrderCodeDetails = await _sfpOrderRepository.GetAsync(x => x.AgentId == sfpOrder.AgentId
                && x.CustomerId == sfpOrder.CustomerId
                && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == DateTime.Now.Date);
                if (existingOrderCodeDetails != null && existingOrderCodeDetails.Count() > 0)
                {
                    if(sfpOrder.Status == existingOrderCodeDetails.First().Status)
                    {
                        sfpOrder.OrderCode = existingOrderCodeDetails.First().OrderCode;
                    }
                }
                await _sfpOrderRepository.UpdateAsync(sfpOrder);
                var schedule = await _sfpCustomizeQuantityService.GetOrderSchedule(sfpOrder.CustomerId, sfpOrder.AgentId, sfpOrder.TransactionDate);
                if(schedule != null && schedule.Count() > 0)
                {
                    foreach(var item in schedule)
                    {
                        item.Status = orderStatus;
                        await _sfpCustomizeQuantityService.UpdateCustomizedQuantity(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateOrderQuantityonDamageReturn(int? customerid,
                                                            int? agentId,
                                                            int? makeModelMasterId,
                                                            DateTime transactionDate,
                                                            int? damagedQuantity)
        {
            try
            {
                var orderDetails = await GetIndividualOrder(customerid ?? 0, agentId ?? 0, transactionDate,"Completed");
                if(orderDetails!=null && orderDetails.Count() > 0)
                {
                    orderDetails = orderDetails.Where(x => x.MakeModelMasterId == makeModelMasterId);
                    if (orderDetails != null && orderDetails.Count() > 0)
                    {
                        SfpOrder sfpOrder = orderDetails.First();
                        var makeModelDetails = await _sfpMakeModelMasterService.GetMakeModel(makeModelMasterId ?? 0);
                        if(makeModelDetails != null && makeModelDetails.Id > 0)
                        {
                            sfpOrder.Quantity = Convert.ToString(Convert.ToInt32(sfpOrder.Quantity) - damagedQuantity);
                            var damagedQuantityPrice = Convert.ToDouble(damagedQuantity) * (Convert.ToDouble(makeModelDetails.Price));
                            sfpOrder.TotalPrice = Convert.ToString(Convert.ToDouble(sfpOrder.TotalPrice) - Convert.ToDouble(damagedQuantityPrice));
                        }
                        await _sfpOrderRepository.UpdateAsync(sfpOrder);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<IEnumerable<SfpOrder>> GetIndividualOrder(int customerid,int agentId, DateTime transactionDate,string status)
        {
            try
            {
                var transactions = await _sfpOrderRepository.GetAsync(x => x.CustomerId == customerid && x.AgentId == agentId && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == transactionDate.Date);
                if (transactions != null && transactions.Count() > 0)
                {
                    if (customerid > 0)
                    {
                        transactions = transactions.Where(x => x.CustomerId == customerid);
                    }
                    if (!string.IsNullOrEmpty(status))
                    {
                        transactions = transactions.Where(x => x.Status == status);
                    }
                    if (transactions != null && transactions.Count() > 0)
                    {
                        return transactions.ToList();
                    }
                }
                return new List<SfpOrder>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpOrder>> GetOrders(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var transactions = await _sfpOrderRepository.GetAsync();
                if (transactions != null && transactions.Count() > 0)
                {
                    if (customerid > 0)
                    {
                        transactions = transactions.Where(x => x.CustomerId == customerid);
                    }
                    if (!string.IsNullOrEmpty(status))
                    {
                        transactions = transactions.Where(x => x.Status == status);
                    }
                    if (fromDate != null && toDate != null)
                    {
                        transactions = transactions.Where(x => (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) >= fromDate.Value.Date && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) <= toDate.Value.Date);
                    }
                    if (deliveryId > 0)
                    {
                        var customers = await _agentCustDeliveryMapService.GetDeliveryAssociatedCustomers(deliveryId);
                        if (customers != null && customers.Count() > 0)
                        {
                            List<int> customerids = customers.Select(x => x.Id).Distinct().ToList();
                            transactions = transactions.Where(x => customerids.Contains(x.CustomerId ?? 0));
                        }
                        else
                        {
                            transactions = new List<SfpOrder>();
                        }
                    }
                    if (agentId > 0)
                    {
                        var customers = await _agentCustDeliveryMapService.GetAgentAssociatedCustomers(agentId);
                        if (customers != null && customers.Count() > 0)
                        {
                            List<int> customerids = customers.Select(x => x.Id).Distinct().ToList();
                            transactions = transactions.Where(x => customerids.Contains(x.CustomerId ?? 0));
                        }
                    }
                    if (transactions != null && transactions.Count() > 0)
                    {
                        return transactions.ToList();
                    }
                }
                return new List<SfpOrder>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpOrder>> GetOrdersforSalesReport(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var transactions = await _sfpOrderRepository.GetAsync();
                if (transactions != null && transactions.Count() > 0)
                {
                    if (customerid > 0)
                    {
                        transactions = transactions.Where(x => x.CustomerId == customerid);
                    }
                    if (!string.IsNullOrEmpty(status))
                    {
                        transactions = transactions.Where(x => x.Status == status);
                    }
                    if (fromDate != null && toDate != null)
                    {
                        transactions = transactions.Where(x => (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) >= fromDate.Value.Date && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) <= toDate.Value.Date);
                    }
                    if (deliveryId > 0)
                    {
                        var customers = await _agentCustDeliveryMapService.GetDeliveryAssociatedCustomers(deliveryId);
                        if (customers != null && customers.Count() > 0)
                        {
                            List<int> customerids = customers.Select(x => x.Id).Distinct().ToList();
                            transactions = transactions.Where(x => customerids.Contains(x.CustomerId ?? 0));
                        }
                    }
                    if (agentId > 0)
                    {
                        var customers = await _agentCustDeliveryMapService.GetAgentAssociatedCustomers(agentId);
                        if (customers != null && customers.Count() > 0)
                        {
                            List<int> customerids = customers.Select(x => x.Id).Distinct().ToList();
                            transactions = transactions.Where(x => customerids.Contains(x.CustomerId ?? 0));
                        }
                    }
                    if (transactions != null && transactions.Count() > 0)
                    {
                        return transactions.ToList();
                    }
                }
                return new List<SfpOrder>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpOrder>> GetOrdersbasedonMakeModel(int makeModelMasterId, int agentId)
        {
            try
            {
                List<SfpOrder> orderInfo = new List<SfpOrder>();
                var associatedCustomers = await _agentCustDeliveryMapService.GetAgentAssociatedCustomers(agentId);
                if (associatedCustomers != null && associatedCustomers.Count() > 0)
                {
                    var customers = associatedCustomers.Select(x => x.Id).ToList();
                    var data = await _sfpOrderRepository.GetAsync(x => x.MakeModelMasterId == makeModelMasterId && customers.Contains((x.CustomerId == null ? 0 : (int)x.CustomerId)));
                    if (data != null && data.Count() > 0)
                    {
                        orderInfo = (data.Where(x => x.Status == "Completed") != null && data.Where(x => x.Status == "Completed").Count() > 0) ? data.Where(x => x.Status == "Completed").ToList() : new List<SfpOrder>();
                    }
                }
                return orderInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CreateOrdersbasedonSchedule()
        {
            try
            {
                var makemodelmasterdata = await _sfpMakeModelMasterService.GetMakeModels();
                var customerAbsentData = await _sfpCustomerAbsentService.GetCustomerAbsentsData();
                var schedules = await _sfpCustomizeQuantityService.GetAllTransactionsbasedonDate(DateTime.Today.Date);
                if (schedules != null && schedules.Count() > 0)
                {
                    foreach (var item in schedules)
                    {
                        var thisCustomerAbsentData = customerAbsentData.Where(x => x.CustomerId == item.CustomerId
                        && (item.TransactionDate == null ? item.TransactionDate : item.TransactionDate.Value.Date) >= (x.AbsentFrom == null ? x.AbsentFrom : x.AbsentFrom.Value.Date)
                        && (item.TransactionDate == null ? item.TransactionDate : item.TransactionDate.Value.Date) <= (x.AbsentTo == null ? x.AbsentTo : x.AbsentTo.Value.Date));
                        if (thisCustomerAbsentData == null || thisCustomerAbsentData.Count() == 0)
                        {
                            var makemodeldata = makemodelmasterdata.First(x => x.Id == item.MakeModelMasterId);
                            SfpOrder order = new SfpOrder();
                            order.CustomerId = item.CustomerId;
                            order.CustomerName = item.CustomerName;
                            order.AgentId = item.AgentId;
                            order.AgentName = item.AgentName;
                            order.DeliveryBoyId = item.DeliveryBoyId;
                            order.DeliveryBoyName = item.DeliveryBoyName;
                            order.TransactionDate = item.TransactionDate;
                            order.MakeModelMasterId = item.MakeModelMasterId;
                            order.Quantity = item.Quantity;
                            order.UnitPrice = item.UnitPrice;
                            order.TotalPrice = item.TotalPrice;
                            order.Status = "Pending";
                            order.OrderCreatedOn = DateTime.Now;
                            await CreateOrder(order);
                            string description = "New Order have been created by " + item.CustomerName
                                + " on " + item.TransactionDate + " for product - " + makemodeldata.ModelName + "(" + makemodeldata.ModelName + ")";
                            await _notificationService.CreateNotification(description, item.AgentId, item.CustomerId, null, (item.TransactionDate == null ? item.TransactionDate : item.TransactionDate.Value.Date),"Order Creation",true,true,true);
                        }
                    }
                }

                // Create Orders for Continuous Customers
                var continuousOrders = await _sfpCustomerQuantityService.GetSchedulebasedonDurationFlag("Continuous");
                if (continuousOrders != null && continuousOrders.Count() > 0)
                {
                    foreach (var item in continuousOrders)
                    {
                        var today = DateTime.Now.Date;
                        var makemodeldata = makemodelmasterdata.First(x => x.Id == item.MakeModelId);
                        var thisCustomerAbsentData = customerAbsentData.Where(x => x.CustomerId == item.CustomerId
                        && today >= (x.AbsentFrom == null ? x.AbsentFrom : x.AbsentFrom.Value.Date)
                        && today <= (x.AbsentTo == null ? x.AbsentTo : x.AbsentTo.Value.Date));
                        if (thisCustomerAbsentData == null || thisCustomerAbsentData.Count() == 0)
                        {
                            SfpOrder order = new SfpOrder();
                            order.CustomerId = item.CustomerId;
                            order.CustomerName = item.CustomerName;
                            order.AgentId = item.AgentId;
                            order.AgentName = item.AgentName;
                            order.DeliveryBoyId = 0;
                            order.DeliveryBoyName = String.Empty;
                            order.TransactionDate = today;
                            order.MakeModelMasterId = item.MakeModelId;
                            order.Quantity = item.Quantity;
                            order.UnitPrice = item.UnitPrice;
                            order.TotalPrice = item.TotalPrice;
                            order.Status = "Pending";
                            order.OrderCreatedOn = DateTime.Now;
                            await CreateOrder(order);
                            string description = "New Order have been created by " + item.CustomerName
                                + " on " + order.TransactionDate + " for product - " + makemodeldata.ModelName + "(" + makemodeldata.ModelName + ")";
                            await _notificationService.CreateNotification(description, item.AgentId, item.CustomerId, null, (order.TransactionDate == null ? order.TransactionDate : order.TransactionDate.Value.Date),"Order Creation",true,true,true);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task RejectPastDatedPendingOrders()
        {
            try
            {
                var today = DateTime.Now.Date;
                var data = await _sfpOrderRepository.GetAsync(x => (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) < today
                && x.Status != "Completed"
                && x.Status != "Rejected");
                if (data != null && data.Count() > 0)
                {
                    foreach (var item in data)
                    {
                        item.Status = "Rejected";
                        item.OrderRejectedOn = DateTime.Now;
                        item.OrderRejectedComments = "Rejected by System";
                        string description = "Customer - " + item.CustomerName + " Order which was placed on " + (item.TransactionDate == null ? "" : item.TransactionDate.Value.ToString("dd-MM-yyyy")) + " have been rejected by system on " + DateTime.Now.ToString("dd-MM-yyyy") +" due to date change.";
                        await UpdateOrder(item);                        
                        await _notificationService.CreateNotification(description, item.AgentId, item.CustomerId, null, (item.TransactionDate == null ? item.TransactionDate : item.TransactionDate.Value.Date), "Order Rejection", true, true, true);
                    }
                }

                var pendingSchedules = await _sfpCustomerQuantityService.GetPastDatedSchedules();
                var pendingSchedulesData = pendingSchedules.Where(x => x.Status != "Approved" && x.Status != "Rejected" && x.DurationFlag != "Continuous");
                if (pendingSchedulesData != null && pendingSchedulesData.Count() > 0)
                {
                    foreach (var item in pendingSchedulesData)
                    {
                        item.Status = "Rejected";
                        await _sfpCustomerQuantityService.UpdateCustomerQuantity(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpOrder>> GetPendingBalanceforCustomer(int customerid,int agentId)
        {
            try
            {
                return await _sfpOrderRepository.GetAsync(x => x.CustomerId == customerid && x.AgentId == agentId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpOrder>> GetOrdersforSync(DateTime? syncDate,int deliveryBoyId)
        {
            try
            {
                List<int> agentIds = new List<int>();
                List<int> customerIds = new List<int>();
                var associatedAgents = await _agentCustDeliveryMapService.GetDeliveryAssociatedAgents(deliveryBoyId);
                if(associatedAgents!=null && associatedAgents.Count() > 0)
                {
                    agentIds = associatedAgents.Select(x => x.Id).Distinct().ToList();
                }
                var associatedCustomers = await _agentCustDeliveryMapService.GetDeliveryAssociatedCustomers(deliveryBoyId);
                if (associatedCustomers != null && associatedCustomers.Count() > 0)
                {
                    customerIds = associatedCustomers.Select(x => x.Id).Distinct().ToList();
                }
                if (agentIds.Count() > 0 && customerIds.Count() > 0)
                {                    
                    if (syncDate == null)
                    {
                        var today = DateTime.Now.Date;
                        var tomorrow = today.AddDays(1);
                        var transactions = await _sfpOrderRepository.GetAsync(x => agentIds.Contains(x.AgentId ?? 0) && customerIds.Contains(x.CustomerId ?? 0) && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) >= today.Date && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) <= tomorrow.Date);
                        if (transactions != null && transactions.Count() > 0)
                        {
                            return transactions.ToList();
                        }
                    }
                    else
                    {
                        var today = (syncDate == null ? DateTime.Now : syncDate.Value);
                        var transactions = await _sfpOrderRepository.GetAsync(x => agentIds.Contains(x.AgentId ?? 0) && customerIds.Contains(x.CustomerId ?? 0) && (x.OrderModifiedOn == null ? x.OrderModifiedOn : x.OrderModifiedOn.Value) >= today);
                        if (transactions != null && transactions.Count() > 0)
                        {
                            return transactions.ToList();
                        }
                    }
                }
                return new List<SfpOrder>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       
    }
}
