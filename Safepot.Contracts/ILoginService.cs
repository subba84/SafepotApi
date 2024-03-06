using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ILoginService
    {
        Task<SfpUser> CheckUser(string username, string password);
    }
}
