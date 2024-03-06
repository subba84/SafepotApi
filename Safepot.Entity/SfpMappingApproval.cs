using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Safepot.Entity
{
    public class SfpMappingApproval
    {
        [Key]
        public int Id { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? ActionPerformedBy { get; set; }
        public string? ActionStatus { get; set; }
        public DateTime? ActionPerformedOn { get; set; }
        public bool? IsForVendorApproval { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerAltMobile { get; set; }
        public string? CustomerEmailId { get; set; }
        public DateTime? StartDate { get; set; }
        public string? StateName { get; set; }
        public string? CityName { get; set; }
        public string? Address { get; set; }
        public string? LandMark { get; set; }
        public string? PinCode { get; set; }
    }
}
