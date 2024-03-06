using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpPaymentReminderService
    {
        public Task CreateAsync(SfpPaymentReminder sfpPaymentReminder);
        public Task<SfpPaymentReminder> GetReminder(int id);
        public Task<IEnumerable<SfpPaymentReminder>> GetReminders(int customerId);
    }
}
