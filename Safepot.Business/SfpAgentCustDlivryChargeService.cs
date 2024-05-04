using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpAgentCustDlivryChargeService : ISfpAgentCustDlivryChargeService
    {
        private readonly ISfpDataRepository<SfpAgentCustDlivryCharge> _deliveryChargeRepository;
        private readonly ISfpUserService _sfpUserService;
        private readonly ISfpCustomerAbsentService _sfpCustomerAbsentService;

        public SfpAgentCustDlivryChargeService(ISfpDataRepository<SfpAgentCustDlivryCharge> deliveryChargeRepository,
            ISfpUserService sfpUserService,
            ISfpCustomerAbsentService sfpCustomerAbsentService)
        {
            _sfpUserService = sfpUserService;
            _sfpCustomerAbsentService = sfpCustomerAbsentService;
            _deliveryChargeRepository = deliveryChargeRepository;
        }

        public async Task DeleteAgentCustDeliveryCharge(int id)
        {
            try
            {
                var masterdata = await _deliveryChargeRepository.GetAsync(x => x.Id == id);
                if (masterdata != null && masterdata.Count() > 0)
                    await _deliveryChargeRepository.DeleteAsync(masterdata.First());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpAgentCustDlivryCharge> GetDeliveryCharge(int id)
        {
            try
            {
                var masterdata = await _deliveryChargeRepository.GetAsync(x => x.Id == id);
                if (masterdata != null && masterdata.Count() > 0)
                    return masterdata.First();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new SfpAgentCustDlivryCharge();
        }

        public async Task<IEnumerable<SfpAgentCustDlivryCharge>> GetDeliveryCharges()
        {
            try
            {
                var masterdata = await _deliveryChargeRepository.GetAsync();
                if (masterdata != null && masterdata.Count() > 0)
                    return masterdata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpAgentCustDlivryCharge>();
        }

        public async Task<double> GetDeliveryChargeforAgentandCustomer(int agentid,int customerid)
        {
            try
            {
                double deliveryCharge = 0;
                var masterdata = await _deliveryChargeRepository.GetAsync(x => x.AgentId == agentid && x.CustomerId == customerid);
                if (masterdata != null && masterdata.Count() > 0)
                {
                    var deliveryChargeforMonth = Convert.ToDouble(masterdata.First().DeliveryCharge);
                    var customerDetails = await _sfpUserService.GetUser(customerid);
                    if (customerDetails.JoinDate != null && customerDetails.JoinDate.HasValue)
                    {
                        var today = DateTime.Now.Date;
                        var customerAbsentData = await _sfpCustomerAbsentService.GetCustomerAbsentDatabasedonCustomerandAgent(customerid, agentid);
                        for (var day = customerDetails.JoinDate.Value.Date; day <= today; day = day.AddDays(1))
                        {
                            var customerAbsentDays = new List<DateTime>();
                            int monthDays = DateTime.DaysInMonth(day.Year, day.Month);
                            if (customerAbsentData != null && customerAbsentData.Count() > 0)
                            {
                                var absentDayliesbtweenAbsents = customerAbsentData.Where(x => day >= (x.AbsentFrom == null ? x.AbsentFrom : x.AbsentFrom.Value.Date) && day <= (x.AbsentTo == null ? x.AbsentTo : x.AbsentTo.Value.Date));
                                if (absentDayliesbtweenAbsents == null || absentDayliesbtweenAbsents.Count() == 0)
                                {
                                    deliveryCharge += Math.Round((deliveryChargeforMonth / monthDays), 0);
                                }
                            }
                            else
                            {
                                deliveryCharge += Math.Round((deliveryChargeforMonth / monthDays), 0);
                            }
                        }
                    }
                }

                return deliveryCharge;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpAgentCustDlivryCharge> GetDeliveryChargeforMonthbasedonAgentandCustomer(int agentid, int customerid)
        {
            try
            {
                var masterdata = await _deliveryChargeRepository.GetAsync(x => x.AgentId == agentid && x.CustomerId == customerid);
                if (masterdata != null && masterdata.Count() > 0)
                    return masterdata.First();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new SfpAgentCustDlivryCharge();
        }

        public async Task<double> GetDeliveryChargeforPeriodbasedonAgentandCustomer(int agentid, int customerid,DateTime fromDate,DateTime toDate)
        {
            try
            {
                double deliveryCharge = 0;
                var masterdata = await _deliveryChargeRepository.GetAsync(x => x.AgentId == agentid && x.CustomerId == customerid);
                if (masterdata != null && masterdata.Count() > 0)
                {
                    var deliveryChargeforMonth = Convert.ToDouble(masterdata.First().DeliveryCharge);
                    var customerDetails = await _sfpUserService.GetUser(customerid);
                    if (customerDetails.JoinDate != null && customerDetails.JoinDate.HasValue)
                    {
                        var today = toDate;
                        var customerAbsentData = await _sfpCustomerAbsentService.GetCustomerAbsentDatabasedonCustomerandAgent(customerid, agentid);
                        for (var day = fromDate.Date; day <= today; day = day.AddDays(1))
                        {
                            var customerAbsentDays = new List<DateTime>();
                            int monthDays = DateTime.DaysInMonth(day.Year, day.Month);
                            if (customerAbsentData != null && customerAbsentData.Count() > 0)
                            {
                                var absentDayliesbtweenAbsents = customerAbsentData.Where(x => day >= (x.AbsentFrom == null ? x.AbsentFrom : x.AbsentFrom.Value.Date) && day <= (x.AbsentTo == null ? x.AbsentTo : x.AbsentTo.Value.Date));
                                if (absentDayliesbtweenAbsents == null || absentDayliesbtweenAbsents.Count() == 0)
                                {
                                    deliveryCharge += Math.Round((deliveryChargeforMonth / monthDays), 0);
                                }
                            }
                            else
                            {
                                deliveryCharge += Math.Round((deliveryChargeforMonth / monthDays), 0);
                            }
                        }
                    }
                }

                return deliveryCharge;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveAgentCustDeliveryCharge(SfpAgentCustDlivryCharge sfpAgentCustDlivryCharge)
        {
            try
            {
                var deliverycharge = await GetDeliveryChargeforMonthbasedonAgentandCustomer(sfpAgentCustDlivryCharge.AgentId ?? 0, sfpAgentCustDlivryCharge.CustomerId ?? 0);
                if(deliverycharge.Id > 0)
                {
                    sfpAgentCustDlivryCharge.Id = deliverycharge.Id;
                    await UpdateAgentCustDeliveryCharge(sfpAgentCustDlivryCharge);
                }
                else
                {
                    await _deliveryChargeRepository.CreateAsync(sfpAgentCustDlivryCharge);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateAgentCustDeliveryCharge(SfpAgentCustDlivryCharge sfpAgentCustDlivryCharge)
        {
            try
            {
                await _deliveryChargeRepository.UpdateAsync(sfpAgentCustDlivryCharge);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
