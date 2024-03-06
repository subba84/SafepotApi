using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpActivityLogService
    {
        Task SaveActivityLog(string activityCategory, string activityDescription, int? objectId, int? activityPerformedBy, string activityPerformerName);
    }
}
