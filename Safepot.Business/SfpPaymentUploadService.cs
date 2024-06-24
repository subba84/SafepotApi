using Microsoft.VisualBasic;
using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpPaymentUploadService : ISfpPaymentUploadService
    {
        private readonly ISfpDataRepository<SfpPaymentUpload> _dataRepository;
        public SfpPaymentUploadService(ISfpDataRepository<SfpPaymentUpload> dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public async Task<IEnumerable<SfpPaymentUpload>> GetPaymentUploads()
        {
            try
            {
                var paymentUploads = await _dataRepository.GetAsync();
                return paymentUploads;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpPaymentUpload> GetPaymentUpload(int id)
        {
            try
            {
                var paymentUploads = await _dataRepository.GetAsync(x=>x.Id == id);
                if(paymentUploads!=null && paymentUploads.Count() > 0)
                {
                    return paymentUploads.First();
                }
                return new SfpPaymentUpload();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SavePaymentUpload(SfpPaymentUpload paymentUpload)
        {
            try
            {
                await _dataRepository.CreateAsync(paymentUpload);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdatePaymentUpload(SfpPaymentUpload paymentUpload)
        {
            try
            {
                await _dataRepository.UpdateAsync(paymentUpload);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeletePaymentUpload(int id)
        {
            try
            {
                SfpPaymentUpload paymentUpload = await GetPaymentUpload(id);
                await _dataRepository.DeleteAsync(paymentUpload);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
