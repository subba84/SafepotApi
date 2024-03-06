using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpOrderService
    {
        Task CreateOrder(SfpOrder sfpOrder);
        Task UpdateOrder(SfpOrder sfpOrder);
        Task<IEnumerable<SfpOrder>> GetOrders(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<SfpOrder>> GetOrdersforSalesReport(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<SfpOrder>> GetIndividualOrder(int customerid, int agentId, DateTime transactionDate,string status);
        Task<IEnumerable<SfpOrder>> GetOrdersbasedonMakeModel(int makeModelMasterId, int agentId);
        Task RejectPastDatedPendingOrders();
        Task CreateOrdersbasedonSchedule();
        Task<IEnumerable<SfpOrder>> GetPendingBalanceforCustomer(int customerid, int agentId);
        Task UpdateOrderQuantityonDamageReturn(int? customerid,
                                               int? agentId,
                                               int? makeModelMasterId,
                                               DateTime transactionDate,
                                               int? damagedQuantity);
    }
}
