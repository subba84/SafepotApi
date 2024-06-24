using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpOtpService
    {
        public Task<string> SaveOtp(string mobileNumber);
        public Task<string> GetOtpbyMobileNumber(string mobileNumber);
        public Task<bool> ValidateOtp(string mobileNumber, string otp);
    }
}
