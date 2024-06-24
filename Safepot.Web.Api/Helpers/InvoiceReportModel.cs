using Safepot.Entity;

namespace Safepot.Web.Api.Helpers
{
    public class InvoiceReportModel
    {
        //public int AgentId { get; set; }
        //public string? AgentName { get; set; }
        //public int CustomerId { get; set; }
        //public string? CustomerName { get; set; }
        public List<MonthlyPrice>? MonthlyPrices { get; set; }
    }

    public class MonthlyPrice
    {
        public string? MonthName { get; set; }
        public double TotalAmount { get; set; }
        public int InvoiceNumber { get; set; }
    }

    public class MonthlyOrderModel
    {
        public int AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? AgentAddress { get; set; }
        public string? AgentMobileNumber { get; set; }
        public string? AgentLandmark { get; set; }
        public string? AgentState { get; set; }
        public string? AgentCity { get; set; }
        public string? AgentPincode { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ValidFrom { get; set; }
        public string? ValidTo { get; set; }
        //public double? TotalAmount { get; set; }
        //public double? DeliveryCharge { get; set; }
        public string? ShopBeside { get; set; }
        public int InvoiceNumber { get; set; }
        public List<Product>? Products { get; set; }
    }

    public class Product
    {
        public string? Title { get; set; }
        public string? Quantity { get; set; }
        public string? Amount { get; set; }
    }
}
