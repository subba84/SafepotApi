using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpUserService
    {
        Task<IEnumerable<SfpUser>> GetAllUsers();
        Task<IEnumerable<SfpUser>> GetAllUsersbasedonRole(int roleId);
        Task<IEnumerable<SfpUser>> GetRolebasedUsers(int roleid);
        Task<SfpUser> GetUserbyMobileNumber(string mobileNumber, bool isCustomer);
        Task<SfpUser> GetUserbyMobileNumberandRole(string mobileNumber,int roleId);
        Task<SfpUser> GetUser(int id);
        Task<IEnumerable<SfpUser>> GetUsers(List<int?> ids);
        Task<int> CreateUser(SfpUser user);
        Task UpdateUser(SfpUser user);
        Task DeleteUser(int id);
    }
}
