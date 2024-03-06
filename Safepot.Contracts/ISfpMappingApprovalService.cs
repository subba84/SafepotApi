using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpMappingApprovalService
    {
        Task<IEnumerable<SfpMappingApproval>> GetMappingApprovalsbasedonAgent(int agentId);
        Task SaveMappingApproval(SfpMappingApproval sfpMappingApproval);
        Task UpdateMappingApproval(SfpMappingApproval sfpMappingApproval);
        Task<IEnumerable<SfpMappingApproval>> GetMappingApprovalsbasedonAgentandStatus(int agentId, string status);
        Task<IEnumerable<SfpMappingApproval>> GetMappingApprovalsbasedonCustomerandStatus(int customerId, string status);
    }
}
