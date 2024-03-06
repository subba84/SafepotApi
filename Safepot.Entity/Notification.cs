using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string? Description { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? DeliveryBoyId { get; set; }
        public string? DeliveryBoyName { get; set; }
        public DateTime? OrderDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool IsRead { get; set; }
        public string? NotificationCategory { get; set; }
        public bool IsForAgent { get; set; }
        public bool IsForCustomer { get; set; }
        public bool IsForDeliveryBoy { get; set; }
    }
}
