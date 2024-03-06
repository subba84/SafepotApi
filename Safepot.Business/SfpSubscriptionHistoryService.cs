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

        public SfpSubscriptionHistoryService(ISfpDataRepository<SfpSubscriptionHistory> subscriptionHistoryRepository)
        {
            _subscriptionHistoryRepository = subscriptionHistoryRepository;
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
    }
}
