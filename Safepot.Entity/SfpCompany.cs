using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpCompany
    {
        [Key]
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public string? Mobile { get; set; }
        public string? EmailId { get; set; }
        public string? PANNumber { get; set; }
        public int? StateId { get; set; }
        public string? StateName { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public string? Address { get; set; }
        public string? LandMark { get; set; }
        public string? Amount { get; set; }
        public string? GST { get; set; }       
        public string? InvoiceType { get; set; }
        public string? TotalAmount { get; set; }
        public string? TransactionId { get; set; }
        public string? PaidVia { get; set; }
        public string? ApprovalStatus { get; set; }
        public string? ActionPerformedBy { get; set; }
        public DateTime? ActionPerformedOn { get; set; }
        public string? ActionRemarks { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? RenewalDate { get; set; }
    }
}
