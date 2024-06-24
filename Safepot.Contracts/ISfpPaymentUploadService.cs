using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpPaymentUploadService
    {
        Task<IEnumerable<SfpPaymentUpload>> GetPaymentUploads();
        Task<SfpPaymentUpload> GetPaymentUpload(int id);
        Task SavePaymentUpload(SfpPaymentUpload paymentUpload);
        Task UpdatePaymentUpload(SfpPaymentUpload paymentUpload);
        Task DeletePaymentUpload(int id);
    }
}
