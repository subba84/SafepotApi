using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpInvoiceService : ISfpInvoiceService
    {
        private readonly ISfpDataRepository<SfpInvoice> _invoiceRepository;
        public SfpInvoiceService(ISfpDataRepository<SfpInvoice> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<int> SaveInvoice(SfpInvoice invoice)
        {
            try
            {
                return await _invoiceRepository.CreateAsync(invoice);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateInvoice(SfpInvoice invoice)
        {
            try
            {
                await _invoiceRepository.UpdateAsync(invoice);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteInvoice(int id)
        {
            try
            {
                if (id > 0)
                {
                    var invoiceDetails = await _invoiceRepository.GetAsync(x => x.Id == id);
                    if(invoiceDetails!=null && invoiceDetails.Count() > 0)
                    {
                        await _invoiceRepository.DeleteAsync(invoiceDetails.First());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpInvoice> GetInvoice(int id)
        {
            try
            {
                if (id > 0)
                {
                    var invoiceDetails = await _invoiceRepository.GetAsync(x => x.Id == id);
                    if (invoiceDetails != null && invoiceDetails.Count() > 0)
                    {
                       return invoiceDetails.First();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new SfpInvoice();
        }

        public async Task<IEnumerable<SfpInvoice>> GetAllInvoices()
        {
            try
            {
               return await _invoiceRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
