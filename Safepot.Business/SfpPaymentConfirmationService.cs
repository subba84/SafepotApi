using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpPaymentConfirmationService : ISfpPaymentConfirmationService
    {
        private readonly ISfpDataRepository<SfpPaymentConfirmation> _paymentConfirmRepository;
        private readonly ISfpAgentCustDeliveryMapService _mappingService;
        private readonly ISfpOrderService _sfpOrderService;
        private readonly ISfpReturnQuantityService _sfpReturnQuantityService;
        private readonly ISfpAgentCustDlivryChargeService _sfpAgentCustDlivryChargeService;
        private readonly ISfpUserService _sfpUserService;
        private readonly ISfpCustomerAbsentService _sfpCustomerAbsentService;
        public SfpPaymentConfirmationService(ISfpDataRepository<SfpPaymentConfirmation> paymentConfirmRepository,
            ISfpAgentCustDeliveryMapService mappingService,
            ISfpOrderService sfpOrderService,
            ISfpReturnQuantityService sfpReturnQuantityService,
            ISfpAgentCustDlivryChargeService sfpAgentCustDlivryChargeService,
            ISfpUserService sfpUserService,
            ISfpCustomerAbsentService sfpCustomerAbsentService)
        {
            _paymentConfirmRepository = paymentConfirmRepository;
            _mappingService = mappingService;
            _sfpOrderService = sfpOrderService;
            _sfpReturnQuantityService = sfpReturnQuantityService;
            _sfpAgentCustDlivryChargeService = sfpAgentCustDlivryChargeService;
            _sfpUserService = sfpUserService;
            _sfpCustomerAbsentService= sfpCustomerAbsentService;
        }

        public async Task<IEnumerable<SfpPaymentConfirmation>> GetPaymentConfirmations()
        {
            try
            {
                return await _paymentConfirmRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public async Task<IEnumerable<SfpPaymentConfirmation>> GetPaymentsforCustomersbasedonDelivery(int deliveryId,int agentId, string status)
        //{
        //    try
        //    {
        //        List<SfpPaymentConfirmation> paymentConfirmations = new List<SfpPaymentConfirmation>();
        //        var customers = await _mappingService.GetAssociatedCustomersbasedonAgentandDelivery(agentId, deliveryId);
        //        if (customers != null && customers.Count() > 0)
        //        {
        //            foreach (var customer in customers)
        //            {
        //                SfpPaymentConfirmation sfpPaymentConfirmation = new SfpPaymentConfirmation();
        //                sfpPaymentConfirmation.CustomerId = customer.Id;
        //                sfpPaymentConfirmation.CustomerName = customer.FirstName + " " + customer.LastName;
        //                var data = await _paymentConfirmRepository.GetAsync(x => x.CustomerId == customer.Id);
        //                if (data != null && data.Count() > 0)
        //                {
        //                    double balancePaid = data.Sum(x => Convert.ToDouble(x.Amount));
        //                    sfpPaymentConfirmation.Amount = Convert.ToString(balancePaid);
        //                }
        //                var amountTobePaid = await GetBalancebasedonCustomerandAgent(customer.Id, agentId);
        //                sfpPaymentConfirmation.BalanceAmount = Convert.ToString(Convert.ToDouble(amountTobePaid) - Convert.ToDouble(sfpPaymentConfirmation.Amount));
        //                if (status == "Pending")
        //                {
        //                    if (sfpPaymentConfirmation.BalanceAmount != "0" && sfpPaymentConfirmation.BalanceAmount != null)
        //                    {
        //                        paymentConfirmations.Add(sfpPaymentConfirmation);
        //                    }
        //                }
        //                else
        //                {
        //                    if (sfpPaymentConfirmation.BalanceAmount == "0")
        //                    {
        //                        paymentConfirmations.Add(sfpPaymentConfirmation);
        //                    }
        //                }
        //            }
        //        }
        //        return paymentConfirmations;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public async Task<IEnumerable<SfpPaymentConfirmation>> GetPaymentsforCustomersbasedonDelivery(int deliveryId, string status)
        {
            try
            {
                List<SfpPaymentConfirmation> paymentConfirmations = new List<SfpPaymentConfirmation>();
                var customers = await _mappingService.GetDeliveryAssociatedCustomers(deliveryId);
                if (customers != null && customers.Count() > 0)
                {
                    foreach (var customer in customers)
                    {
                        var agentData = await _mappingService.GetCustomerAssociatedAgents(customer.Id);
                        if(agentData!=null && agentData.Count() > 0)
                        {
                            foreach(var agent in agentData)
                            {
                                SfpPaymentConfirmation sfpPaymentConfirmation = new SfpPaymentConfirmation();
                                sfpPaymentConfirmation.CustomerId = customer.Id;
                                sfpPaymentConfirmation.CustomerName = customer.FirstName + " " + customer.LastName;
                                sfpPaymentConfirmation.AgentId = agent.Id;
                                sfpPaymentConfirmation.AgentName = agent.FirstName + " " + agent.LastName;
                                var data = await _paymentConfirmRepository.GetAsync(x => x.CustomerId == customer.Id && x.AgentId == agent.Id);
                                if (data != null && data.Count() > 0)
                                {
                                    double balancePaid = data.Sum(x => Convert.ToDouble(x.Amount));
                                    sfpPaymentConfirmation.Amount = Convert.ToString(balancePaid);
                                }
                                var amountTobePaid = await GetBalancebasedonCustomerandAgent(customer.Id, agent.Id);
                                // amount paid by customer already deducted in the above method, so we are adding it again here to balance the amount
                                amountTobePaid = amountTobePaid + Convert.ToDouble(sfpPaymentConfirmation.Amount ?? "0");
                                var deliveryCharge = await _sfpAgentCustDlivryChargeService.GetDeliveryChargeforAgentandCustomer(agent.Id, customer.Id);
                                sfpPaymentConfirmation.BalanceAmount = Convert.ToString(Convert.ToDouble(amountTobePaid + deliveryCharge) - Convert.ToDouble(sfpPaymentConfirmation.Amount));
                                if (status == "Pending")
                                {
                                    if (sfpPaymentConfirmation.BalanceAmount != "0" && sfpPaymentConfirmation.BalanceAmount != null)
                                    {
                                        paymentConfirmations.Add(sfpPaymentConfirmation);
                                    }
                                }
                                else
                                {
                                    if (sfpPaymentConfirmation.BalanceAmount == "0")
                                    {
                                        paymentConfirmations.Add(sfpPaymentConfirmation);
                                    }
                                }
                            }
                        }
                    }
                }
                return paymentConfirmations;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpPaymentConfirmation>> GetPaymentHistorybasedonAgentandCustomer(int agentId, int customerId)
        {
            try
            {
                var data = await _paymentConfirmRepository.GetAsync(x => x.AgentId == agentId && x.CustomerId == customerId);                
                return data ?? new List<SfpPaymentConfirmation>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpPaymentConfirmation>> GetPaymentConfirmationbyCustomer(int customerId,string status)
        {
            try
            {
                List<SfpPaymentConfirmation> paymentConfirmations = new List<SfpPaymentConfirmation>();
                if (customerId > 0)
                {
                    var customer = await _sfpUserService.GetUser(customerId);
                    var agentData = await _mappingService.GetCustomerAssociatedAgents(customer.Id);
                    if (agentData != null && agentData.Count() > 0)
                    {
                        foreach (var agent in agentData)
                        {
                            SfpPaymentConfirmation sfpPaymentConfirmation = new SfpPaymentConfirmation();
                            sfpPaymentConfirmation.CustomerId = customer.Id;
                            sfpPaymentConfirmation.CustomerName = customer.FirstName + " " + customer.LastName;
                            sfpPaymentConfirmation.AgentId = agent.Id;
                            sfpPaymentConfirmation.AgentName = agent.FirstName + " " + agent.LastName;
                            var data = await _paymentConfirmRepository.GetAsync(x => x.CustomerId == customer.Id && x.AgentId == agent.Id);
                            if (data != null && data.Count() > 0)
                            {
                                double balancePaid = data.Sum(x => Convert.ToDouble(x.Amount));
                                sfpPaymentConfirmation.Amount = Convert.ToString(balancePaid);
                            }
                            var amountTobePaid = await GetBalancebasedonCustomerandAgent(customer.Id, agent.Id);
                            // amount paid by customer already deducted in the above method, so we are adding it again here to balance the amount
                            amountTobePaid = amountTobePaid + Convert.ToDouble(sfpPaymentConfirmation.Amount ?? "0");
                            var deliveryCharge = await _sfpAgentCustDlivryChargeService.GetDeliveryChargeforAgentandCustomer(agent.Id, customer.Id);
                            sfpPaymentConfirmation.BalanceAmount = Convert.ToString(Convert.ToDouble(amountTobePaid + deliveryCharge) - Convert.ToDouble(sfpPaymentConfirmation.Amount));
                            if (status == "Pending")
                            {
                                if (sfpPaymentConfirmation.BalanceAmount != "0" && sfpPaymentConfirmation.BalanceAmount != null)
                                {
                                    sfpPaymentConfirmation.Amount = sfpPaymentConfirmation.BalanceAmount;
                                    paymentConfirmations.Add(sfpPaymentConfirmation);
                                }
                            }
                            else
                            {
                                if (sfpPaymentConfirmation.BalanceAmount == "0")
                                {
                                    paymentConfirmations.Add(sfpPaymentConfirmation);
                                }
                            }
                        }
                    }
                }
                return paymentConfirmations;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<double> GetBalancebasedonCustomerandAgent(int customerId,int agentId)
        {
            try
            {
                double balanceAmount = 0; double returnQuantityAmount = 0;double amountPaidbyCustomer = 0;double deliveryCharge = 0;
                var orderData = await _sfpOrderService.GetPendingBalanceforCustomer(customerId, agentId);
                var returnQuantity = await _sfpReturnQuantityService.GetReturnRequestsDatabasedonCustomer(customerId, agentId);
                if (orderData != null && orderData.Count() > 0)
                {
                    balanceAmount = orderData.Sum(x => Convert.ToDouble(x.TotalPrice));
                    //if(returnQuantity!=null && returnQuantity.Count() > 0)
                    //{
                    //    foreach(var item in returnQuantity)
                    //    {
                    //        returnQuantityAmount = returnQuantityAmount + ((string.IsNullOrEmpty(item.Quantity) ? 0 : Convert.ToDouble(item.Quantity)) * (string.IsNullOrEmpty(item.Price) ? 0 : Convert.ToDouble(item.Price)));
                    //    }                       
                    //}
                }
                var customertransations = await _paymentConfirmRepository.GetAsync(x => x.CustomerId == customerId && x.AgentId == agentId);
                if(customertransations!=null && customertransations.Count() > 0)
                {
                    amountPaidbyCustomer = customertransations.Sum(x => (x.Amount == null ? 0 : Convert.ToDouble(x.Amount)));
                }

                deliveryCharge = 0;// await _sfpAgentCustDlivryChargeService.GetDeliveryChargeforAgentandCustomer(agentId, customerId);
                //balanceAmount = balanceAmount - returnQuantityAmount - amountPaidbyCustomer;
                balanceAmount = amountPaidbyCustomer - balanceAmount - returnQuantityAmount;
                balanceAmount += Math.Round(deliveryCharge,0);
                return balanceAmount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpPaymentConfirmation> GetPaymentConfirmation(int id)
        {
            try
            {
                var data = await _paymentConfirmRepository.GetAsync(x => x.Id == id);
                if (data != null && data.Count() > 0)
                    return data.First();
                return new SfpPaymentConfirmation();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> SavePaymentConfirmation(SfpPaymentConfirmation sfpPaymentConfirmation)
        {
            try
            {
                return await _paymentConfirmRepository.CreateAsync(sfpPaymentConfirmation);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdatePaymentConfirmation(SfpPaymentConfirmation sfpPaymentConfirmation)
        {
            try
            {
                await _paymentConfirmRepository.UpdateAsync(sfpPaymentConfirmation);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeletePaymentConfirmation(SfpPaymentConfirmation sfpPaymentConfirmation)
        {
            try
            {
                await _paymentConfirmRepository.DeleteAsync(sfpPaymentConfirmation);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetMinimumBalanceCustomersbasedonAgent(int agentId)
        {
            try
            {
                List<SfpUser> customers = new List<SfpUser>();
                var agentMappedCustomers = await _mappingService.GetAgentAssociatedCustomers(agentId);
                if(agentMappedCustomers != null && agentMappedCustomers.Count() > 0)
                {
                    customers = agentMappedCustomers.ToList();
                    foreach (var customer in customers)
                    {
                        customer.ApprovalStatus = string.Empty;
                        double balanceAmount = 0; double returnQuantityAmount = 0; double amountPaidbyCustomer = 0;
                        var orderData = await _sfpOrderService.GetPendingBalanceforCustomer(customer.Id, agentId);
                        var returnQuantity = await _sfpReturnQuantityService.GetReturnRequestsDatabasedonCustomer(customer.Id, agentId);
                        if (orderData != null && orderData.Count() > 0)
                        {
                            balanceAmount = orderData.Sum(x => Convert.ToDouble(x.TotalPrice));
                        }
                        var customertransations = await _paymentConfirmRepository.GetAsync(x => x.CustomerId == customer.Id && x.AgentId == agentId);
                        if (customertransations != null && customertransations.Count() > 0)
                        {
                            amountPaidbyCustomer = customertransations.Sum(x => (x.Amount == null ? 0 : Convert.ToDouble(x.Amount)));
                        }
                        balanceAmount = amountPaidbyCustomer - balanceAmount - returnQuantityAmount;
                        // Using approval status column for carrying balance to controller
                        customer.ApprovalStatus = Convert.ToString(balanceAmount);
                    }
                }
                return customers;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
