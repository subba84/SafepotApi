using Safepot.Entity;

namespace Safepot.Web.Api.Helpers
{
    public class OrderDetailsModel
    {
        public List<SfpCustomizeQuantity> Products { get; set; }
        public DateTime? TransactionDate { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? Mobile { get; set; }
        public string? AltMobile { get; set; }
        public string? EmailId { get; set; }
        public string? StateName { get; set; }
        public string? CityName { get; set; }
        public string? Address { get; set; }
        public string? LandMark { get; set; }
        public string? PinCode { get; set; }
        public string? Status { get; set; }

        public string? AgentMobile { get; set; }
        public string? AgentAltMobile { get; set; }
        public string? AgentEmailId { get; set; }
        public string? AgentStateName { get; set; }
        public string? AgentCityName { get; set; }
        public string? AgentAddress { get; set; }
        public string? AgentLandMark { get; set; }
        public string? AgentPinCode { get; set; }
    }
}
