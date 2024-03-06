using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpRoleMasterService : ISfpRoleMasterService
    {
        private readonly ISfpDataRepository<SfpRoleMaster> _roleRepository;

        public SfpRoleMasterService(ISfpDataRepository<SfpRoleMaster> roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<SfpRoleMaster>> GetRoles()
        {
            try
            {
                return await _roleRepository.GetAsync();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task AddRole(string rolename)
        {
            try
            {
                SfpRoleMaster roleMaster = new SfpRoleMaster();
                roleMaster.RoleName = rolename;
                await _roleRepository.CreateAsync(roleMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
