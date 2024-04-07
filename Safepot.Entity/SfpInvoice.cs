using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpInvoice
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyState { get; set; }
        public string? CompanyCity { get; set; }
        public string? GSTNumber { get; set; }
        public string? CustomerGSTNumber { get; set; }
        public string? CustomerPAN { get; set; }
        public string? TotalAmount { get; set; }
        public string? Amount { get; set; }
        public string? GST { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
