using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpCustomizeQuantity
    {
        [Key]
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? TransactionDate { get; set; }
        public int? MakeModelMasterId { get; set; }
        public string? Quantity { get; set; }
        public string? UnitPrice { get; set; }
        public string? TotalPrice { get; set; }
        public string? UnitQuantity { get; set; }
        public string? Status { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public string? ModifierName { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool? IsAbsent { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? MakeName { get; set; }
        public string? ModelName { get; set; }
        public string? UomName { get; set; }
        public string? Price { get; set; }
        public int? DeliveryBoyId { get; set; }
        public string? DeliveryBoyName { get; set; }
        public string? OrderCreatedBy { get; set; }
    }
}
