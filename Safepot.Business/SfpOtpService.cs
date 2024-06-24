using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpOtpService : ISfpOtpService
    { 
        private readonly ISfpDataRepository<SfpOtp> _sfpDataRepository;
        public SfpOtpService(ISfpDataRepository<SfpOtp> sfpDataRepository)
        {
            _sfpDataRepository = sfpDataRepository;
        }

        public async Task<string> SaveOtp(string mobileNumber)
        {
            try
            {
                Random rnd = new Random();
                string otp = rnd.Next(0, 1000000).ToString("D6");
                SfpOtp sfpOtp= new SfpOtp();
                sfpOtp.Otp = otp;
                sfpOtp.Mobile = mobileNumber;
                sfpOtp.CreatedOn = DateTime.Now;
                sfpOtp.IsValidated = false;
                await _sfpDataRepository.CreateAsync(sfpOtp);
                return otp;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetOtpbyMobileNumber(string mobileNumber)
        {
            try
            {
                var otpDetails = await _sfpDataRepository.GetAsync(x => x.Mobile == mobileNumber && (x.IsValidated == null || x.IsValidated == false));
                if(otpDetails != null && otpDetails.Count() > 0)
                {
                    return otpDetails.Last().Otp ?? "";
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return string.Empty;
        }

        public async Task<bool> ValidateOtp(string mobileNumber, string otp)
        {
            try
            {
                var otpDetails = await _sfpDataRepository.GetAsync(x => x.Mobile == mobileNumber && x.Otp == otp && (x.IsValidated == null || x.IsValidated == false));
                if (otpDetails != null && otpDetails.Count() > 0)
                {
                    SfpOtp sfpOtp = otpDetails.Last();
                    sfpOtp.IsValidated = true;
                    await _sfpDataRepository.UpdateAsync(sfpOtp);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
    }
}
