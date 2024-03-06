using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpPaymentReminderService : ISfpPaymentReminderService
    {
        private readonly ISfpDataRepository<SfpPaymentReminder> _sfpDataRepository;
        public SfpPaymentReminderService(ISfpDataRepository<SfpPaymentReminder> sfpDataRepository)
        {
            _sfpDataRepository = sfpDataRepository;
        }

        public async Task<IEnumerable<SfpPaymentReminder>> GetReminders(int customerId)
        {
            try
            {
                return await _sfpDataRepository.GetAsync(x=>x.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpPaymentReminder> GetReminder(int id)
        {
            try
            {
                var data = await _sfpDataRepository.GetAsync(x => x.Id == id);
                if (data != null && data.Count() > 0)
                    return data.First();
                return new SfpPaymentReminder();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CreateAsync(SfpPaymentReminder sfpPaymentReminder)
        {
            try
            {
                await _sfpDataRepository.CreateAsync(sfpPaymentReminder);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
