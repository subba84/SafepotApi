using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpUser
    {
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Mobile { get; set; }
        public string? AltMobile { get; set; }
        public string? EmailId { get; set; }
        public int? StateId { get; set; }
        public string? StateName { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public string? Address { get; set; }
        public string? LandMark { get; set; }
        public string? PinCode { get; set; }
        public string? CompanyName { get; set; }
        public string? GSTNumber { get; set; }
        public string? PANNumber { get; set; }
        public string? GeoSelection { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ApprovalStatus { get; set; }
        public int? ActionPerformedBy { get; set; }
        public DateTime? ActionPerformedOn { get; set; }
        public string? ActionRemarks { get; set; }
        public DateTime? JoinDate { get; set; }
        public string? SubscriptionPrice { get; set; }
        public DateTime? RenewalDate { get; set; }
        public string? BillingType { get; set; }
        public string? UserCode { get; set; }
        public bool? IsProfileCompleted { get; set; }
        public DateTime? StartDate { get; set; }
        //public List<SfpRoleMaster>? UserRoles { get; set; }
        public bool? IsMobileAppInstalled { get; set; }
    }
}
