using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpSubscriptionHistoryService : ISfpSubscriptionHistoryService
    {
        private readonly ISfpDataRepository<SfpSubscriptionHistory> _subscriptionHistoryRepository;
        private readonly ISfpDataRepository<SfpUser> _userRepository;

        public SfpSubscriptionHistoryService(ISfpDataRepository<SfpSubscriptionHistory> subscriptionHistoryRepository,
            ISfpDataRepository<SfpUser> userRepository)
        {
            _subscriptionHistoryRepository = subscriptionHistoryRepository;
            _userRepository= userRepository;
        }
        public async Task<IEnumerable<SfpSubscriptionHistory>> GetAllSubscriptionHistory(int agentid)
        {
            try
            {
                return await _subscriptionHistoryRepository.GetAsync(x => x.AgentId == agentid);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveSubscriptionHistory(SfpSubscriptionHistory history)
        {
            try
            {
                await _subscriptionHistoryRepository.CreateAsync(history);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateSubscriptionHistory(SfpSubscriptionHistory history)
        {
            try
            {
                await _subscriptionHistoryRepository.UpdateAsync(history);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteSubscriptionHistory(int id)
        {
            try
            {
                var masterdata = await _subscriptionHistoryRepository.GetAsync(x => x.Id == id);
                if (masterdata != null && masterdata.Count() > 0)
                    await _subscriptionHistoryRepository.DeleteAsync(masterdata.First());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> IsAgentServiceExpires(int agentId)
        {
            try
            {
                var subscriptionDetails = await _subscriptionHistoryRepository.GetAsync(x => x.AgentId == agentId);
                if(subscriptionDetails!=null && subscriptionDetails.Count() > 0)
                {
                    SfpSubscriptionHistory sfpSubscriptionHistory = subscriptionDetails.OrderByDescending(x=>x.Id).First();
                    DateTime fromDate = Convert.ToDateTime(sfpSubscriptionHistory.PlanStartDate);
                    DateTime toDate = Convert.ToDateTime(sfpSubscriptionHistory.PlanEndDate);
                    DateTime today = DateTime.Now.Date;
                    if(today >= fromDate && today <= toDate)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }

        public async Task<List<SfpSubscriptionHistory>> GetAgentsexpiredinThreeMonths()
        {
            List<SfpSubscriptionHistory> users = new List<SfpSubscriptionHistory>();
            try
            {                
                var subscriptionDetails = await _subscriptionHistoryRepository.GetAsync();
                if (subscriptionDetails != null && subscriptionDetails.Count() > 0)
                {
                    
                    List<int?> agentIds = subscriptionDetails.Select(x=>x.AgentId).Distinct().ToList();
                    var agents = await _userRepository.GetAsync(x => agentIds.Contains(x.Id));
                    if(agents!=null && agents.Count() > 0)
                    {
                        foreach(var agent in agents)
                        {
                            SfpSubscriptionHistory sfpSubscriptionHistory = subscriptionDetails.Where(x=>x.AgentId == agent.Id).OrderByDescending(x => x.Id).First();                            
                            DateTime fromDate = DateTime.Now;
                            DateTime toDate = Convert.ToDateTime(sfpSubscriptionHistory.PlanEndDate);
                            int duration = (toDate - fromDate).Days;
                            sfpSubscriptionHistory.Duration = duration.ToString();
                            if (duration <= 90)
                            {
                                sfpSubscriptionHistory.AgentName = sfpSubscriptionHistory.AgentName + "|" + agent.CompanyName + "|" + agent.Mobile;
                                users.Add(sfpSubscriptionHistory);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return users;
        }

        public async Task<List<SfpSubscriptionHistory>> GetAgentSubscriptions()
        {
            List<SfpSubscriptionHistory> users = new List<SfpSubscriptionHistory>();
            try
            {
                var subscriptionDetails = await _subscriptionHistoryRepository.GetAsync();
                if (subscriptionDetails != null && subscriptionDetails.Count() > 0)
                {

                    List<int?> agentIds = subscriptionDetails.Select(x => x.AgentId).Distinct().ToList();
                    var agents = await _userRepository.GetAsync(x => agentIds.Contains(x.Id));
                    if (agents != null && agents.Count() > 0)
                    {
                        foreach (var agent in agents)
                        {
                            SfpSubscriptionHistory sfpSubscriptionHistory = subscriptionDetails.Where(x => x.AgentId == agent.Id).OrderByDescending(x => x.Id).First();
                            DateTime fromDate = Convert.ToDateTime(sfpSubscriptionHistory.PlanStartDate);
                            DateTime toDate = Convert.ToDateTime(sfpSubscriptionHistory.PlanEndDate);
                            int duration = (toDate - fromDate).Days;
                            sfpSubscriptionHistory.Duration = duration.ToString();
                            sfpSubscriptionHistory.AgentName = sfpSubscriptionHistory.AgentName + "|" + agent.CompanyName + "|" + agent.Mobile;
                            users.Add(sfpSubscriptionHistory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return users;
        }
    }
}
