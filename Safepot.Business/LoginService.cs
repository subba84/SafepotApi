using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class LoginService : ILoginService
    {
        private readonly ISfpDataRepository<SfpUser> _userRepository;

        public LoginService(ISfpDataRepository<SfpUser> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<SfpUser> CheckUser(string username,string password)
        {
            try
            {
                var result = await _userRepository.GetAsync();
                if(result !=null && result.Count() > 0)
                {
                    var loginuser = result.Where(x => x.UserName == username && x.Password == password);
                    if(loginuser!=null && loginuser.Count() > 0)
                    {
                        var user = loginuser.First();
                        if(string.IsNullOrEmpty(user.EmailId) || string.IsNullOrEmpty(user.Mobile))
                        {
                            user.IsProfileCompleted = false;
                        }
                        else
                        {
                            user.IsProfileCompleted = true;
                        }
                        return user;
                    }
                    return new SfpUser() { IsProfileCompleted = false };
                }
                return new SfpUser() { IsProfileCompleted = false };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
