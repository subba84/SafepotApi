using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpCustomizedQuantityService : ISfpCustomizedQuantityService
    {
        private readonly ISfpDataRepository<SfpCustomizeQuantity> _customisedQuantityRepository;
        private readonly ISfpDataRepository<SfpCustomerQuantity> _customerQuantityRepository;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ISfpCustomerAbsentService _sfpCustomerAbsentService;
        private readonly ISfpAgentCustDeliveryMapService _agentCustDeliveryMapService;

        public SfpCustomizedQuantityService(ISfpDataRepository<SfpCustomizeQuantity> customisedQuantityRepository,
            ISfpAgentCustDeliveryMapService agentCustDeliveryMapService,
            ISfpMakeModelMasterService sfpMakeModelMasterService,
            ISfpCustomerAbsentService sfpCustomerAbsentService,
            ISfpDataRepository<SfpCustomerQuantity> customerQuantityRepository)
        {
            _customisedQuantityRepository = customisedQuantityRepository;
            _agentCustDeliveryMapService = agentCustDeliveryMapService;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
            _sfpCustomerAbsentService = sfpCustomerAbsentService;
            _customerQuantityRepository = customerQuantityRepository;
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetAllTransactions()
        {
            try
            {
                return await _customisedQuantityRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetAllTransactionsbasedonDate(DateTime? date)
        {
            try
            {
                var today = (date == null ? DateTime.Now.Date : date.Value.Date);
                var tomorrow = today.AddDays(1);
                return await _customisedQuantityRepository.GetAsync(x=> (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) >= today &&  (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) <= tomorrow);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetAllTransactionsbasedonUser(int agentId,int customerId,int deliveryBoyId,DateTime? fromDate,DateTime? toDate)
        {
            try
            {
                var data = await GetOrders(customerId, agentId, deliveryBoyId, "", fromDate,toDate);
                if(data!=null && data.Count() > 0)
                {
                    var consolidatedData = from c in data
                                           group c by new
                                           {
                                               c.CustomerId,
                                               c.MakeModelMasterId,
                                               c.TransactionDate
                                           } into gcs
                                           select new SfpCustomizeQuantity()
                                           {
                                               CustomerId = gcs.Key.CustomerId,
                                               MakeModelMasterId = gcs.Key.MakeModelMasterId,
                                               TransactionDate = gcs.Key.TransactionDate,
                                               Quantity = gcs.Sum(x => Convert.ToInt32(x.Quantity)).ToString(),
                                           };
                    return consolidatedData;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpCustomizeQuantity> GetTransactionbasedonId(int id)
        {
            try
            {
                var data = await _customisedQuantityRepository.GetAsync(x=>x.Id == id);
                if(data!=null && data.Count() > 0)
                {
                    return data.First();
                }
                else
                {
                    return new SfpCustomizeQuantity();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetAllTransactionsbasedonMakeModelMaster(int makeModelMasterId,int agentId)
        {
            try
            {
                List<SfpCustomizeQuantity> orderInfo = new List<SfpCustomizeQuantity>();
                var associatedCustomers = await _agentCustDeliveryMapService.GetAgentAssociatedCustomers(agentId);
                if(associatedCustomers!=null && associatedCustomers.Count() > 0)
                {
                    var customers = associatedCustomers.Select(x=>x.Id).ToList();
                    var data = await _customisedQuantityRepository.GetAsync(x => x.MakeModelMasterId == makeModelMasterId && customers.Contains((x.CustomerId == null ? 0 : (int)x.CustomerId))/* && x.Status == "Completed"*/);
                    if(data!=null && data.Count() > 0)
                    {
                        orderInfo = (data.Where(x => x.Status == "Completed") != null && data.Where(x => x.Status == "Completed").Count() > 0) ? data.Where(x=>x.Status == "Completed").ToList() : new List<SfpCustomizeQuantity>();
                    }
                }
                return orderInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetTransactionsbasedonCustomer(int customerid,int agentid, int makeModelMasterId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                List<SfpCustomizeQuantity> sfpCustomizeQuantities = new List<SfpCustomizeQuantity>();
                List<DateTime> absentDates = new List<DateTime>();
                var customerAbsentData = await _sfpCustomerAbsentService.GetAbsentDatabasedonCustomer(customerid,null,null);
                if (customerAbsentData != null && customerAbsentData.Count() > 0)
                {
                    foreach (var item in customerAbsentData.ToList())
                    {
                        for (var date = (item.AbsentFrom == null ? DateTime.MinValue : item.AbsentFrom.Value.Date); date <= (item.AbsentTo == null ? DateTime.MinValue : item.AbsentTo.Value.Date); date = date.AddDays(1))
                        {
                            absentDates.Add(date);
                        }
                    }
                }
                var data = await _customerQuantityRepository.GetAsync(x => x.CustomerId == customerid && x.AgentId == agentid && x.MakeModelId == makeModelMasterId/* && fromDate.Date >= (x.FromDate == null ? x.FromDate : x.FromDate.Value.Date)  && (x.ToDate == null ? x.ToDate : x.ToDate.Value.Date) <= toDate.Date*/);
                if(data != null && data.Count() > 0)
                {
                    var customizeData = await _customisedQuantityRepository.GetAsync(x => x.CustomerId == customerid && x.AgentId == agentid && x.MakeModelMasterId == makeModelMasterId);
                    var makeModelDetails = await _sfpMakeModelMasterService.GetMakeModel(makeModelMasterId);
                   


                    for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                    {
                        SfpCustomizeQuantity sfpCustomizeQuantity = new SfpCustomizeQuantity();
                        if (absentDates.Contains(date))
                        {
                            sfpCustomizeQuantity.CustomerId = customerid;
                            sfpCustomizeQuantity.MakeModelMasterId = makeModelMasterId;
                            sfpCustomizeQuantity.TransactionDate = date;
                            sfpCustomizeQuantity.UnitPrice = makeModelDetails.Price;
                            sfpCustomizeQuantity.TotalPrice = "0";
                            sfpCustomizeQuantity.IsAbsent = true;
                            sfpCustomizeQuantities.Add(sfpCustomizeQuantity);
                        }
                        //else if ((data == null || data.Count() == 0))
                        //{
                            //sfpCustomizeQuantity.CustomerId = customerid;
                            //sfpCustomizeQuantity.MakeModelMasterId = makeModelMasterId;
                            //sfpCustomizeQuantity.TransactionDate = date;
                            //sfpCustomizeQuantity.Quantity = "0";
                            //sfpCustomizeQuantity.UnitPrice = makeModelDetails.Price;
                            //sfpCustomizeQuantity.TotalPrice = "0";
                            //sfpCustomizeQuantities.Add(sfpCustomizeQuantity);
                        //}
                        else
                        {
                            if (data.First().DurationFlag == "Continuous")
                            {
                                var fromDt = data.First().FromDate;
                                if (date >= (fromDt == null ? null : fromDt.Value.Date))
                                {
                                    var customertransactiondata = customizeData.Where(x => (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == date.Date);
                                    if (customertransactiondata == null || customertransactiondata.Count() == 0)
                                    {
                                        sfpCustomizeQuantity.CustomerId = customerid;
                                        sfpCustomizeQuantity.MakeModelMasterId = makeModelMasterId;
                                        sfpCustomizeQuantity.TransactionDate = date;
                                        sfpCustomizeQuantity.Quantity = data.First().Quantity;
                                        sfpCustomizeQuantity.UnitPrice = makeModelDetails.Price;
                                        sfpCustomizeQuantity.TotalPrice = data.First().TotalPrice;
                                        sfpCustomizeQuantities.Add(sfpCustomizeQuantity);
                                    }
                                    else
                                    {
                                        sfpCustomizeQuantities.Add(customertransactiondata.First());
                                    }
                                }
                            }
                            else
                            {
                                var customerSelectedDateRange = data.Last();
                                if (date >= (customerSelectedDateRange.FromDate == null ? null : customerSelectedDateRange.FromDate.Value.Date) && date <= (customerSelectedDateRange.ToDate == null ? null : customerSelectedDateRange.ToDate.Value.Date))
                                {
                                    var customertransactiondata = customizeData.Where(x => (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == date.Date);
                                    if (customertransactiondata == null || customertransactiondata.Count() == 0)
                                    {
                                        sfpCustomizeQuantity.CustomerId = customerid;
                                        sfpCustomizeQuantity.MakeModelMasterId = makeModelMasterId;
                                        sfpCustomizeQuantity.TransactionDate = date;
                                        sfpCustomizeQuantity.Quantity = data.First().Quantity;
                                        sfpCustomizeQuantity.UnitPrice = makeModelDetails.Price;
                                        sfpCustomizeQuantity.TotalPrice = data.First().TotalPrice;
                                        sfpCustomizeQuantities.Add(sfpCustomizeQuantity);
                                    }
                                    else
                                    {
                                        sfpCustomizeQuantities.Add(customertransactiondata.First());
                                    }
                                }
                            }
                            
                            
                        }
                    }
                }
                
                return sfpCustomizeQuantities;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public async Task<IEnumerable<SfpCustomizeQuantity>> GetTransactionsbasedonCustomer(int customerid,int makeModelMasterId,DateTime fromDate,DateTime toDate)
        //{
        //    try
        //    {
        //        List<SfpCustomizeQuantity> sfpCustomizeQuantities = new List<SfpCustomizeQuantity>();
        //        List<DateTime> absentDates = new List<DateTime>();
        //        var customerAbsentData = await _sfpCustomerAbsentService.GetAbsentDatabasedonCustomer(customerid);
        //        if (customerAbsentData != null && customerAbsentData.Count() > 0)
        //        {
        //            foreach(var item in customerAbsentData.ToList())
        //            {
        //                for (var date = (item.AbsentFrom == null ? DateTime.MinValue : item.AbsentFrom.Value.Date); date <= (item.AbsentTo == null ? DateTime.MinValue : item.AbsentTo.Value.Date); date = date.AddDays(1))
        //                {
        //                    absentDates.Add(date);
        //                }
        //            }
        //        }
        //        var data = await _customisedQuantityRepository.GetAsync(x => x.CustomerId == customerid && x.MakeModelMasterId == makeModelMasterId && x.TransactionDate.Value.Date >= fromDate.Date && x.TransactionDate.Value.Date <= toDate.Date);
        //        var makeModelDetails = await _sfpMakeModelMasterService.GetMakeModel(makeModelMasterId);
        //        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        //        {
        //            SfpCustomizeQuantity sfpCustomizeQuantity = new SfpCustomizeQuantity();
        //            if (absentDates.Contains(date))
        //            {
        //                sfpCustomizeQuantity.CustomerId = customerid;
        //                sfpCustomizeQuantity.MakeModelMasterId = makeModelMasterId;
        //                sfpCustomizeQuantity.TransactionDate = date;
        //                sfpCustomizeQuantity.UnitPrice = makeModelDetails.Price;
        //                sfpCustomizeQuantity.TotalPrice = "0";
        //                sfpCustomizeQuantity.IsAbsent = true;
        //                sfpCustomizeQuantities.Add(sfpCustomizeQuantity);
        //            }
        //            else if (data == null || data.Count() == 0)
        //            {
        //                sfpCustomizeQuantity.CustomerId = customerid;
        //                sfpCustomizeQuantity.MakeModelMasterId = makeModelMasterId;
        //                sfpCustomizeQuantity.TransactionDate = date;
        //                sfpCustomizeQuantity.Quantity = "0";
        //                sfpCustomizeQuantity.UnitPrice = makeModelDetails.Price;
        //                sfpCustomizeQuantity.TotalPrice = "0";
        //                sfpCustomizeQuantities.Add(sfpCustomizeQuantity);
        //            }
        //            else
        //            {
        //                var customertransactiondata = data.Where(x => x.TransactionDate.Value.Date == date.Date);
        //                if (customertransactiondata == null || customertransactiondata.Count() == 0)
        //                {
        //                    sfpCustomizeQuantity.CustomerId = customerid;
        //                    sfpCustomizeQuantity.MakeModelMasterId = makeModelMasterId;
        //                    sfpCustomizeQuantity.TransactionDate = date;
        //                    sfpCustomizeQuantity.Quantity = "0";
        //                    sfpCustomizeQuantity.UnitPrice = makeModelDetails.Price;
        //                    sfpCustomizeQuantity.TotalPrice = "0";
        //                    sfpCustomizeQuantities.Add(sfpCustomizeQuantity);
        //                }
        //                else
        //                {
        //                    sfpCustomizeQuantities.Add(customertransactiondata.First());
        //                }
        //            }
        //        }
        //        return sfpCustomizeQuantities;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetAllTransactionsbasedonCustomer(int customerid)
        {
            try
            {
                return await _customisedQuantityRepository.GetAsync(x => x.CustomerId == customerid);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetExistingCustomizeQtybasedonDate(int? customerId, DateTime? fromDate, DateTime? toDate, int? makeModelMasterId)
        {
            try
            {
                var data = await _customisedQuantityRepository.GetAsync(x => x.CustomerId == customerId && x.MakeModelMasterId == makeModelMasterId && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) >= (fromDate == null ? fromDate : fromDate.Value.Date) && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) <= (toDate == null ? toDate : toDate.Value.Date));
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetOrders(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                //var transactions = await _customisedQuantityRepository.GetAsync(x=>(x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == DateTime.Now.Date);
                var transactions = await _customisedQuantityRepository.GetAsync();
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
                return new List<SfpCustomizeQuantity>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetOrderSchedule(int? customerId,int? agentId, DateTime? transactionDate)
        {
            try
            {
                var transactions = await _customisedQuantityRepository.GetAsync(x=>x.CustomerId == customerId && x.AgentId == agentId && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == (transactionDate == null ? transactionDate : transactionDate.Value.Date));
                if (transactions != null && transactions.Count() > 0)
                {
                    return transactions.ToList();
                }
                return new List<SfpCustomizeQuantity>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpCustomizeQuantity> GetIndividualOrderbasedonProduct(int customerid,int agentId, DateTime transactionDate,int makeModelMasterId)
        {
            try
            {
                var transactions = await _customisedQuantityRepository.GetAsync(x => x.CustomerId == customerid && (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == transactionDate.Date && x.AgentId == agentId && x.MakeModelMasterId == makeModelMasterId);
                if (transactions != null && transactions.Count() > 0)
                {
                    return transactions.First();
                }
                return new SfpCustomizeQuantity();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetOrdersforSalesReport(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                //var transactions = await _customisedQuantityRepository.GetAsync(x => (x.TransactionDate == null ? x.TransactionDate : x.TransactionDate.Value.Date) == DateTime.Now.Date);
                var transactions = await _customisedQuantityRepository.GetAsync();
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
                return new List<SfpCustomizeQuantity>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity)
        {
            try
            {
                await _customisedQuantityRepository.CreateAsync(sfpCustomizeQuantity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity)
        {
            try
            {
                await _customisedQuantityRepository.UpdateAsync(sfpCustomizeQuantity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity)
        {
            try
            {
                await _customisedQuantityRepository.DeleteAsync(sfpCustomizeQuantity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
