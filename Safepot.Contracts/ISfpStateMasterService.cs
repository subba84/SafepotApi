using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpStateMasterService
    {
        Task<IEnumerable<SfpStateMaster>> GetStates();

        Task SaveState(SfpStateMaster state);
    }
}
