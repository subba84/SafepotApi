using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpCustomerInvoiceService
    {
        Task<IEnumerable<SfpCustomerInvoice>> GetCustomerInvoices(int customerId);
        Task<SfpCustomerInvoice> GetCustomerInvoice(int year, int month, int customerId);
        Task<int> CreateCustomerInvoice(SfpCustomerInvoice sfpCustomerInvoice);
        Task<int> GetCustomerInvoiceId(int year, int month, int customerId);
        Task<SfpCustomerInvoice> GetCustomerInvoice(int id);
    }
}
