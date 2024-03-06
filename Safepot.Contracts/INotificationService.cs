using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface INotificationService
    {
        public Task<IEnumerable<Notification>> GetNotifications(int agentId, int customerId, int deliveryBoyId);

        public Task CreateNotification(string description,
            int? agentId,
            int? customerId,
            int? deliveryBoyId,
            DateTime? orderDate,
            string category,
            bool isForAgent = false,
            bool isForCustomer = false,
            bool isForDeliveryBoy = false);

        public Task UpdateNotification(Notification notification);
    }
}
