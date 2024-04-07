using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpInvoiceService
    {
        Task<int> SaveInvoice(SfpInvoice invoice);
        Task UpdateInvoice(SfpInvoice invoice);
        Task DeleteInvoice(int id);
        Task<SfpInvoice> GetInvoice(int id);
        Task<IEnumerable<SfpInvoice>> GetAllInvoices();
    }
}
