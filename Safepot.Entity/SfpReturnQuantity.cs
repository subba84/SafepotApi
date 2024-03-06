using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpReturnQuantity
    {
        [Key]
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public int? MakeModelId { get; set; }
        public string? Quantity { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? ReturnComments { get; set; }
        public string? Status { get; set; }
        public int? ActionPerformedBy { get; set; }
        public DateTime? ActionPerformedOn { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? TransactionId { get; set; }
        public string? MakeName { get; set; }
        public string? ModelName { get; set; }
        public string? UomName { get; set; }
        public string? Price { get; set; }
        public string? UnitQuantity { get; set; }
        public bool? IsForVendorApproval { get; set; }
        public string? RefundType { get; set; }
    }
}
