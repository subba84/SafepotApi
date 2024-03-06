using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface IUserRoleMapService
    {
        Task<IEnumerable<SfpUserRoleMap>> GetAllRolesbyUser(int userId);
        Task SaveUserRole(SfpUserRoleMap sfpUserRoleMap);
        Task UpdateUserRole(SfpUserRoleMap sfpUserRoleMap);
        Task DeleteUserRole(int id);
        Task<IEnumerable<SfpUserRoleMap>> GetRolesofUser(string mobileNumber);
    }
}
