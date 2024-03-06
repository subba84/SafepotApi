using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpCustomerQuantityService
    {
        public Task<IEnumerable<SfpCustomerQuantity>> GetExistingCustomerQtybasedonDate(int? customerId, DateTime? fromDate, DateTime? toDate, int? makeModelMasterId);
        public Task<IEnumerable<SfpCustomerQuantity>> GetQuantitiesforCustomer();
        public Task<IEnumerable<SfpCustomerQuantity>> GetProductsbasedonCustomer(int customerId,int agentId,string status);
        public Task<SfpCustomerQuantity> GetQuantitiesforCustomer(int customerid);
        public Task SaveCustomerQuantity(SfpCustomerQuantity sfpCustomerQuantity);
        public Task UpdateCustomerQuantity(SfpCustomerQuantity sfpCustomerQuantity);
        public Task DeleteCustomerQuantity(SfpCustomerQuantity sfpCustomerQuantity);
        public Task<IEnumerable<SfpCustomerQuantity>> GetSegregatedProductDataforCustomersbasedonDeliveryBoy(int deliveryBoyId);
        Task<IEnumerable<SfpCustomerQuantity>> GetOrdersforAgentbasedonStatus(int agentId, string status);
        Task<IEnumerable<SfpCustomerQuantity>> GetOrdersforCustomerbasedonStatus(int customerId, string status);
        Task<SfpCustomerQuantity> GetQuantityforCustomer(int id);
        Task<IEnumerable<SfpCustomerQuantity>> GetAnonymousOrders(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<SfpCustomerQuantity>> GetSchedulebasedonDurationFlag(string durationFlag);
        Task PerformApprovalAction(SfpCustomerQuantity approvalData);
        Task<IEnumerable<SfpCustomerQuantity>> GetPastDatedSchedules();
    }
}
