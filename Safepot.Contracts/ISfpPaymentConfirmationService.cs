using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpPaymentConfirmationService
    {
        public Task<IEnumerable<SfpPaymentConfirmation>> GetPaymentConfirmations();
        public Task<SfpPaymentConfirmation> GetPaymentConfirmation(int id);
        public Task<int> SavePaymentConfirmation(SfpPaymentConfirmation sfpPaymentConfirmation);
        public Task UpdatePaymentConfirmation(SfpPaymentConfirmation sfpPaymentConfirmation);
        public Task DeletePaymentConfirmation(SfpPaymentConfirmation sfpPaymentConfirmation);
        //public Task<IEnumerable<SfpPaymentConfirmation>> GetPaymentsforCustomersbasedonDelivery(int deliveryId, int agentId, string status);
        public Task<IEnumerable<SfpPaymentConfirmation>> GetPaymentsforCustomersbasedonDelivery(int deliveryId, string status);
        public Task<IEnumerable<SfpPaymentConfirmation>> GetPaymentConfirmationbyCustomer(int customerId, string status);
        //public Task<SfpPaymentConfirmation> GetBalancebasedonCustomerandAgent(int customerId,int agentId);
        Task<double> GetBalancebasedonCustomerandAgent(int customerId, int agentId);
        public Task<IEnumerable<SfpPaymentConfirmation>> GetPaymentHistorybasedonAgentandCustomer(int agentId, int customerId);
        public Task<IEnumerable<SfpUser>> GetMinimumBalanceCustomersbasedonAgent(int agentId);
    }
}
