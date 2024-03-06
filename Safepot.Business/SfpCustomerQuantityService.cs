using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpCustomerQuantityService : ISfpCustomerQuantityService
    {
        private readonly ISfpDataRepository<SfpCustomerQuantity> _customerQuantityRepository;
        private readonly ISfpDataRepository<SfpCustomizeQuantity> _customizeQuantityRepository;
        private readonly ISfpDataRepository<SfpAgentCustDeliveryMap> _mappingRepository;
        private readonly ISfpCustomerAbsentService _sfpCustomerAbsentService;
        private readonly ISfpAgentCustDeliveryMapService _agentCustDeliveryMapService;        
        //private readonly ISfpOrderService _sfpOrderService;

        public SfpCustomerQuantityService(ISfpDataRepository<SfpCustomerQuantity> customerQuantityRepository,
            ISfpDataRepository<SfpAgentCustDeliveryMap> mappingRepository, ISfpCustomerAbsentService sfpCustomerAbsentService,
            ISfpAgentCustDeliveryMapService agentCustDeliveryMapService,
            //ISfpOrderService sfpOrderService, 
            ISfpDataRepository<SfpCustomizeQuantity> customizeQuantityRepository)
        {
            _customerQuantityRepository = customerQuantityRepository;
            _mappingRepository = mappingRepository;
            _sfpCustomerAbsentService = sfpCustomerAbsentService;
            _agentCustDeliveryMapService = agentCustDeliveryMapService;            
            //_sfpOrderService = sfpOrderService;
            _customizeQuantityRepository = customizeQuantityRepository;
        }

        public async Task<IEnumerable<SfpCustomerQuantity>> GetQuantitiesforCustomer()
        {
            try
            {
                return await _customerQuantityRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomerQuantity>> GetSchedulebasedonDurationFlag(string durationFlag)
        {
            try
            {
                var orders = await _customerQuantityRepository.GetAsync();
                return orders.Where(x=>x.DurationFlag == "Continuous");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpCustomerQuantity> GetQuantityforCustomer(int id)
        {
            try
            {
                var data = await _customerQuantityRepository.GetAsync(x=>x.Id == id);
                if (data != null && data.Count() > 0)
                    return data.First();
                return new SfpCustomerQuantity();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomerQuantity>> GetProductsbasedonCustomer(int customerId,int agentId,string status)
        {
            try
            {
                var data = await _customerQuantityRepository.GetAsync(x=>x.CustomerId == customerId && x.AgentId == agentId);
                if(data!=null && data.Count() > 0)
                {
                    return data.Where(x=>x.Status == status);
                }
                return new List<SfpCustomerQuantity>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomerQuantity>> GetOrdersforAgentbasedonStatus(int agentId,string status)
        {
            try
            {
                var data = await _customerQuantityRepository.GetAsync(x => x.AgentId == agentId);
                if (data != null && data.Count() > 0)
                {
                    return data.Where(x => x.Status == status && x.OrderCreatedBy == "Customer");
                }
                return new List<SfpCustomerQuantity>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomerQuantity>> GetOrdersforCustomerbasedonStatus(int customerId, string status)
        {
            try
            {
                var data = await _customerQuantityRepository.GetAsync(x => x.CustomerId == customerId);
                if (data != null && data.Count() > 0)
                {
                    return data.Where(x => x.Status == status/* && x.OrderCreatedBy == "Agent"*/);
                }
                return new List<SfpCustomerQuantity>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpCustomerQuantity> GetQuantitiesforCustomer(int customerid)
        {
            try
            {
                var data = await _customerQuantityRepository.GetAsync(x => x.CustomerId == customerid);
                if (data != null && data.Count() > 0)
                    return data.First();
                return new SfpCustomerQuantity();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveCustomerQuantity(SfpCustomerQuantity sfpCustomerQuantity)
        {
            try
            {
                await _customerQuantityRepository.CreateAsync(sfpCustomerQuantity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomerQuantity>> GetExistingCustomerQtybasedonDate(int? customerId, DateTime? fromDate,DateTime? toDate, int? makeModelMasterId)
        {
            try
            {
                var data = await _customerQuantityRepository.GetAsync(x => x.CustomerId == customerId && x.MakeModelId == makeModelMasterId && (x.FromDate == null ? x.FromDate : x.FromDate.Value.Date) >= (fromDate == null ? fromDate : fromDate.Value.Date) && (x.ToDate == null ? x.ToDate : x.ToDate.Value.Date) <= (toDate == null ? toDate : toDate.Value.Date));               
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateCustomerQuantity(SfpCustomerQuantity sfpCustomerQuantity)
        {
            try
            {
                await _customerQuantityRepository.UpdateAsync(sfpCustomerQuantity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteCustomerQuantity(SfpCustomerQuantity sfpCustomerQuantity)
        {
            try
            {
                await _customerQuantityRepository.DeleteAsync(sfpCustomerQuantity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomerQuantity>> GetSegregatedProductDataforCustomersbasedonDeliveryBoy(int deliveryBoyId)
        {
            try
            {
                var data = await _mappingRepository.GetAsync(x => x.DeliveryId == deliveryBoyId);
                if(data!=null && data.Count() > 0)
                {
                    var customers = data.Where(x=>x.CustomerId!=null && x.CustomerId!=0).Select(x => x.CustomerId).Distinct().ToList();
                    if(customers!=null && customers.Count() > 0)
                    {
                        var productData = await _customerQuantityRepository.GetAsync(x => customers.Contains(x.CustomerId));
                        if(productData!=null && productData.Count() > 0)
                        {
                            var consolidatedData = from c in productData
                                                   group c by new
                                                   {
                                                       c.MakeModelId,
                                                       c.MakeName,
                                                       c.ModelName,
                                                       c.UomName,
                                                       c.Price,
                                                       c.CustomerId,
                                                       c.CustomerName
                                                   } into gcs
                                                   select new SfpCustomerQuantity()
                                                   {
                                                       MakeModelId = gcs.Key.MakeModelId,
                                                       MakeName = gcs.Key.MakeName,
                                                       ModelName = gcs.Key.ModelName,
                                                       UomName = gcs.Key.UomName,
                                                       CustomerId = gcs.Key.CustomerId,
                                                       CustomerName = gcs.Key.CustomerName,
                                                       Price = gcs.Key.Price,
                                                       Quantity = Convert.ToString(gcs.Sum(x => Convert.ToInt32(x.Quantity)))
                                                   };
                            return consolidatedData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpCustomerQuantity>();
        }

        public async Task<IEnumerable<SfpCustomerQuantity>> GetAnonymousOrders(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                //var transactions = await _customisedQuantityRepository.GetAsync(x=>(x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == DateTime.Now.Date);
                var transactions = await _customerQuantityRepository.GetAsync();
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
                        transactions = transactions.Where(x => (x.FromDate == null ? x.FromDate : x.FromDate.Value.Date) >= fromDate.Value.Date && (x.ToDate == null ? x.ToDate : x.ToDate.Value.Date) <= toDate.Value.Date);
                    }
                    if (deliveryId > 0)
                    {
                        //transactions = transactions.Where(x => x.DeliveryBoyId == deliveryId);
                        var customers = await _agentCustDeliveryMapService.GetDeliveryAssociatedCustomers(deliveryId);
                        if (customers != null && customers.Count() > 0)
                        {
                            List<int> customerids = customers.Select(x => x.Id).Distinct().ToList();
                            transactions = transactions.Where(x => customerids.Contains(x.CustomerId ?? 0));
                        }
                    }
                    if (agentId > 0)
                    {
                        //transactions = transactions.Where(x => x.AgentId == agentId);
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
                return new List<SfpCustomerQuantity>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task PerformApprovalAction(SfpCustomerQuantity approvalData)
        {
            try
            {
                if ((approvalData.Status == "Approved" || approvalData.Status == "Partial Approved") && approvalData.DurationFlag != "Continuous")
                {
                    // Allow transactions only status is approved...
                    for (var day = approvalData.FromDate; day <= approvalData.ToDate; day = day.Value.AddDays(1))
                    {
                        SfpCustomizeQuantity sfpCustomizeQuantity = new SfpCustomizeQuantity();
                        sfpCustomizeQuantity.MakeModelMasterId = approvalData.MakeModelId;
                        sfpCustomizeQuantity.Price = approvalData.Price;
                        sfpCustomizeQuantity.MakeName = approvalData.MakeName;
                        sfpCustomizeQuantity.ModelName = approvalData.ModelName;
                        sfpCustomizeQuantity.UomName = approvalData.UomName;
                        sfpCustomizeQuantity.UnitQuantity = approvalData.UnitQuantity;
                        sfpCustomizeQuantity.TransactionDate = day.Value.Date;
                        sfpCustomizeQuantity.CustomerId = approvalData.CustomerId;
                        sfpCustomizeQuantity.CustomerName = approvalData.CustomerName;
                        sfpCustomizeQuantity.Status = approvalData.Status;
                        sfpCustomizeQuantity.TotalPrice = approvalData.TotalPrice;
                        sfpCustomizeQuantity.UnitPrice = approvalData.UnitPrice;
                        sfpCustomizeQuantity.Quantity = approvalData.Quantity;
                        sfpCustomizeQuantity.CreatedBy = approvalData.CustomerId;
                        sfpCustomizeQuantity.CreatorName = approvalData.CustomerName;
                        sfpCustomizeQuantity.CreatedOn = DateTime.Now;
                        sfpCustomizeQuantity.AgentId = approvalData.AgentId;
                        sfpCustomizeQuantity.AgentName = approvalData.AgentName;
                        sfpCustomizeQuantity.OrderCreatedBy = approvalData.OrderCreatedBy;
                        if (approvalData.Status == "Partial Approved" && sfpCustomizeQuantity.TransactionDate == DateTime.Now.Date)
                        {
                            // We need to skip this schedule entry because when we do partial approval, we will not deliver the order today
                        }
                        else
                        {
                            await _customizeQuantityRepository.CreateAsync(sfpCustomizeQuantity);
                        }
                    }
                }
                if (approvalData.Id > 0)
                {
                    await UpdateCustomerQuantity(approvalData);
                }               
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<SfpCustomerQuantity>> GetPastDatedSchedules()
        {
            try
            {
                var today = DateTime.Now.Date;
                var data = await _customerQuantityRepository.GetAsync(x => (x.ToDate == null ? x.ToDate : x.ToDate.Value.Date) < today);
                if (data != null && data.Count() > 0)
                    return data;
                return new List<SfpCustomerQuantity>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
