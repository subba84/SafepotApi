using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpMakeMasterService
    {
        Task<IEnumerable<SfpMakeMaster>> GetMakes();
        Task<SfpMakeMaster> GetMake(int id);
        Task SaveMake(SfpMakeMaster sfpMakeMaster);
    }
}
