using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpCustomizedQuantityService
    {
        Task<IEnumerable<SfpCustomizeQuantity>> GetAllTransactions();
        Task<IEnumerable<SfpCustomizeQuantity>> GetAllTransactionsbasedonDate(DateTime? date);
        Task<SfpCustomizeQuantity> GetTransactionbasedonId(int id);
        Task<IEnumerable<SfpCustomizeQuantity>> GetAllTransactionsbasedonMakeModelMaster(int makeModelMasterId,int agentId);
        Task<IEnumerable<SfpCustomizeQuantity>> GetTransactionsbasedonCustomer(int customerid,int agentid, int makeModelMasterId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<SfpCustomizeQuantity>> GetAllTransactionsbasedonCustomer(int customerid);
        Task SaveCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity);
        Task UpdateCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity);
        Task DeleteCustomizedQuantity(SfpCustomizeQuantity sfpCustomizeQuantity);
        Task<IEnumerable<SfpCustomizeQuantity>> GetOrders(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<SfpCustomizeQuantity>> GetOrderSchedule(int? customerId, int? agentId, DateTime? transactionDate);
        Task<IEnumerable<SfpCustomizeQuantity>> GetExistingCustomizeQtybasedonDate(int? customerId, DateTime? fromDate, DateTime? toDate, int? makeModelMasterId);
        Task<IEnumerable<SfpCustomizeQuantity>> GetOrdersforSalesReport(int customerid, int agentId, int deliveryId, string? status, DateTime? fromDate, DateTime? toDate);
        Task<SfpCustomizeQuantity> GetIndividualOrderbasedonProduct(int customerid, int agentId, DateTime transactionDate, int makeModelMasterId);
    }
}
