using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpPaymentUpload
    {
        [Key]
        public int Id { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? MobileNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? RelativePath { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
