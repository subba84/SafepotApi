using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class NotificationService : INotificationService
    {
        private readonly ISfpDataRepository<Notification> _sfpNotificationRepository;
        private readonly ISfpAgentCustDeliveryMapService _sfpAgentCustDeliveryMapService;
        public NotificationService(ISfpDataRepository<Notification> sfpNotificationRepository, ISfpAgentCustDeliveryMapService sfpAgentCustDeliveryMapService)
        {
            _sfpNotificationRepository = sfpNotificationRepository;
            _sfpAgentCustDeliveryMapService = sfpAgentCustDeliveryMapService;
        }

        public async Task<IEnumerable<Notification>> GetNotifications(int userId,int userRoleId)
        {
            try
            {
                List<Notification> notifications = new List<Notification>();
                var data = await _sfpNotificationRepository.GetAsync(x=>x.IsRead == false);
                if(data!=null && data.Count() > 0)
                {
                    if(userId > 0 && userRoleId == AppRoles.Agent)
                    {
                        data = data.Where(x => x.AgentId == userId && x.IsForAgent == true);
                    }
                    if(userId > 0 && userRoleId == AppRoles.Customer)
                    {
                        data = data.Where(x => x.CustomerId == userId && x.IsForCustomer == true);
                    }
                    if(userId > 0 && userRoleId == AppRoles.Delivery)
                    {
                        data = data.Where(x => x.CustomerId == userId && x.IsForDeliveryBoy == true);
                    }
                }
                return ((data!=null && data.Count() > 0) ? data.ToList() : notifications);
            }
            catch(Exception ex)
            {
                throw ex;
            }            
        }

        //public async Task CreateNotification(Notification notification)
        //{
        //    try
        //    {
        //        await _sfpNotificationRepository.CreateAsync(notification);
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public async Task CreateNotification(string description,
            int? agentId,
            int? customerId,
            int? deliveryBoyId,
            DateTime? orderDate,
            string category,
            bool isForAgent=false,
            bool isForCustomer=false,
            bool isForDeliveryBoy = false)
        {
            try
            {
                Notification notification = new Notification();
                notification.Description = description;
                notification.AgentId = agentId;
                notification.CustomerId = customerId;
                notification.DeliveryBoyId = deliveryBoyId;
                notification.OrderDate = orderDate;
                notification.CreatedOn = DateTime.Now;
                notification.IsRead = false;
                notification.NotificationCategory = category;
                notification.IsForAgent = isForAgent;
                notification.IsForCustomer = isForCustomer;
                notification.IsForDeliveryBoy = isForDeliveryBoy;
                await _sfpNotificationRepository.CreateAsync(notification);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateNotification(Notification notification)
        {
            try
            {
                await _sfpNotificationRepository.UpdateAsync(notification);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
