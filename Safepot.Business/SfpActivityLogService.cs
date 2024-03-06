using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpActivityLogService : ISfpActivityLogService
    {
        private readonly ISfpDataRepository<SfpActivityLog> _activityLogRepository;

        public SfpActivityLogService(ISfpDataRepository<SfpActivityLog> activityLogRepository)
        {
            _activityLogRepository = activityLogRepository;
        }
        public async Task SaveActivityLog(string activityCategory, 
            string activityDescription, 
            int? objectId,
            int? activityPerformedBy, 
            string? activityPerformerName)
        {
            try
            {
                SfpActivityLog log = new SfpActivityLog();
                log.ActivityCategory = activityCategory;
                log.ActivityDescription = activityDescription;
                log.ObjectId = objectId;
                log.ActivityPerformedBy = activityPerformedBy;
                log.ActivityPerformerName = activityPerformerName;
                log.ActivityPerformedOn = DateTime.Now;
                await _activityLogRepository.CreateAsync(log);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
