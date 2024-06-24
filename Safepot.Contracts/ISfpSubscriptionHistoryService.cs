using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpSubscriptionHistoryService
    {
        Task SaveSubscriptionHistory(SfpSubscriptionHistory history);
        Task<IEnumerable<SfpSubscriptionHistory>> GetAllSubscriptionHistory(int agentid);
        Task UpdateSubscriptionHistory(SfpSubscriptionHistory history);
        Task DeleteSubscriptionHistory(int id);
        Task<List<SfpSubscriptionHistory>> GetAgentsexpiredinThreeMonths();
        Task<List<SfpSubscriptionHistory>> GetAgentSubscriptions();

    }
}
