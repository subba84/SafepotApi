using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class UserRoleMapService : IUserRoleMapService
    {
        private readonly ISfpDataRepository<SfpUserRoleMap> _userRoleRepository;
        private readonly ISfpDataRepository<SfpUser> _userRepository;

        public UserRoleMapService(ISfpDataRepository<SfpUserRoleMap> userRoleRepository, ISfpDataRepository<SfpUser> userRepository)
        {
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<SfpUserRoleMap>> GetAllRolesbyUser(int userId)
        {
            try
            {
                return await _userRoleRepository.GetAsync(x => x.UserId == userId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpUserRoleMap>> GetRolesofUser(string mobileNumber)
        {
            try
            {
                List<SfpUserRoleMap> users = new List<SfpUserRoleMap>();
                var data = await _userRepository.GetAsync();
                if(data!=null && data.Count() > 0)
                {
                    data = data.Where(x => x.Mobile == mobileNumber);
                    if(data!=null && data.Count() > 0)
                    {
                        List<int> userids = data.Select(x => x.Id).Distinct().ToList();
                        if(userids!=null && userids.Count() > 0)
                        {
                            foreach(var user in userids)
                            {
                                var userRoleMap = await _userRoleRepository.GetAsync(x => x.UserId == user);
                                if (userRoleMap != null)
                                {
                                    var userDetails = data.Where(x => x.Id == user).First();
                                    SfpUserRoleMap map = new SfpUserRoleMap();
                                    map = userRoleMap.First();
                                    map.UserName = userDetails.FirstName + " " + userDetails.LastName;
                                    map.RoleName = userDetails.RoleName;
                                    users.Add(map);
                                }
                            }
                        }
                        return users.DistinctBy(x=>x.RoleId).ToList(); //await _userRoleRepository.GetAsync(x => userids.Contains(Convert.ToInt32(x.UserId)));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpUserRoleMap>();
        }

        public async Task SaveUserRole(SfpUserRoleMap sfpUserRoleMap)
        {
            try
            {
                await _userRoleRepository.CreateAsync(sfpUserRoleMap);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateUserRole(SfpUserRoleMap sfpUserRoleMap)
        {
            try
            {
                await _userRoleRepository.UpdateAsync(sfpUserRoleMap);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteUserRole(int id)
        {
            try
            {
                var data = await _userRoleRepository.GetAsync(x => x.Id == id);
                if (data != null && data.Count() > 0)
                    await _userRoleRepository.DeleteAsync(data.First());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
