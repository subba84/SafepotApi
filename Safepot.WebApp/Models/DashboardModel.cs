using Safepot.Entity;

namespace Safepot.WebApp.Models
{
    public class DashboardModel
    {
        public List<SfpSubscriptionHistory>? AgentsExpiredinThreeMonths { get; set; }
        public List<SfpSubscriptionHistory>? AgentsSubscriptions { get; set; }
    }
}
