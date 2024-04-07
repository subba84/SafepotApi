using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpOrder
    {
        [Key]
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public int? DeliveryBoyId { get; set; }
        public string? DeliveryBoyName { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? Quantity { get; set; }
        public int? MakeModelMasterId { get; set; }
        public string? UnitPrice { get; set; }
        public string? TotalPrice { get; set; }
        public string? Status { get; set; }
        public int? OrderAcceptedBy { get; set; }
        public DateTime? OrderAcceptedOn { get; set; }
        public int? OrderCompletedBy { get; set; }
        public DateTime? OrderCompletedOn { get; set; }
        public int? OrderRejectedBy { get; set; }
        public DateTime? OrderRejectedOn { get; set; }
        public string? OrderRejectedComments { get; set; }
        public DateTime? OrderCreatedOn { get; set; }
        public DateTime? OrderModifiedOn { get; set; }
        public bool? IsOrderSync { get; set; }
        public DateTime? OrderSyncDate { get; set; }
        public string? OrderCode { get; set; }
    }
}
