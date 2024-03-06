using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpCityMasterService
    {
        Task<IEnumerable<SfpCityMaster>> GetCities(int stateid);
        Task SaveCity(SfpCityMaster city);
    }
}
