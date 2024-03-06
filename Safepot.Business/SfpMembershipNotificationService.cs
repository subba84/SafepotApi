using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpMembershipNotificationService : ISfpMembershipNotificationService
    {
        private readonly ISfpDataRepository<SfpMembershipNotification> _membershipRepository;

        public SfpMembershipNotificationService(ISfpDataRepository<SfpMembershipNotification> membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }
        public async Task CreateMembershipNotification(SfpMembershipNotification sfpMembershipNotification)
        {
            try
            {
                await _membershipRepository.CreateAsync(sfpMembershipNotification);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
