using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpCustomerInvoiceService : ISfpCustomerInvoiceService
    {
        private readonly ISfpDataRepository<SfpCustomerInvoice> _customerInvoiceRepository;

        public SfpCustomerInvoiceService(ISfpDataRepository<SfpCustomerInvoice> customerInvoiceRepository)
        {
            _customerInvoiceRepository = customerInvoiceRepository;
        }

        public async Task<SfpCustomerInvoice> GetCustomerInvoice(int id)
        {
            try
            {
                var data = await _customerInvoiceRepository.GetAsync(x => x.Id == id);
                if(data!=null && data.Count() > 0)
                {
                    return data.First();
                }
                else
                {
                    return new SfpCustomerInvoice();
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomerInvoice>> GetCustomerInvoices(int customerId)
        {
            try
            {
                var data = await _customerInvoiceRepository.GetAsync(x => x.CustomerId == customerId);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpCustomerInvoice> GetCustomerInvoice(int year,int month,int customerId)
        {
            try
            {
                var data = await _customerInvoiceRepository.GetAsync(x => x.InvoiceYear == year && x.InvoiceMonth == month && x.CustomerId == customerId);
                if(data == null || data.Count() == 0)
                    return new SfpCustomerInvoice();
                return data.First();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> CreateCustomerInvoice(SfpCustomerInvoice sfpCustomerInvoice)
        {
            try
            {
                await _customerInvoiceRepository.CreateAsync(sfpCustomerInvoice);
                return sfpCustomerInvoice.Id;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> GetCustomerInvoiceId(int year,int month,int customerId)
        {
            try
            {
                SfpCustomerInvoice sfpCustomerInvoice = await GetCustomerInvoice(year, month, customerId);
                if (sfpCustomerInvoice.Id > 0)
                    return sfpCustomerInvoice.Id;
                return await CreateCustomerInvoice(new SfpCustomerInvoice { InvoiceYear = year,InvoiceMonth = month,CustomerId = customerId });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
