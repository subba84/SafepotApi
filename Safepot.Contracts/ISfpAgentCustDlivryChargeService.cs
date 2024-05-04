using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpAgentCustDlivryChargeService
    {
         Task DeleteAgentCustDeliveryCharge(int id);
         Task<SfpAgentCustDlivryCharge> GetDeliveryCharge(int id);
         Task<IEnumerable<SfpAgentCustDlivryCharge>> GetDeliveryCharges();
         Task<SfpAgentCustDlivryCharge> GetDeliveryChargeforMonthbasedonAgentandCustomer(int agentid, int customerid);
         Task<double> GetDeliveryChargeforAgentandCustomer(int agentid, int customerid);
         Task SaveAgentCustDeliveryCharge(SfpAgentCustDlivryCharge sfpAgentCustDlivryCharge);
         Task UpdateAgentCustDeliveryCharge(SfpAgentCustDlivryCharge sfpAgentCustDlivryCharge);
         Task<double> GetDeliveryChargeforPeriodbasedonAgentandCustomer(int agentid, int customerid, DateTime fromDate, DateTime toDate);
    }
}
