using Microsoft.VisualBasic;
using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;

namespace Safepot.Business
{
    public class SfpUserService : ISfpUserService
    {
        private readonly ISfpDataRepository<SfpUser> _userRepository;
        private readonly ISfpDataRepository<SfpUserRoleMap> _userRoleRepository;
        private readonly ISfpDataRepository<SfpAgentCustDeliveryMap> _agentCustDeliveryMapRepository;
        private readonly ISfpDataRepository<SfpMappingApproval> _mappingApprovalRepository;

        public SfpUserService(ISfpDataRepository<SfpUser> userRepository, ISfpDataRepository<SfpUserRoleMap> userRoleRepository, ISfpDataRepository<SfpAgentCustDeliveryMap> agentCustDeliveryMapRepository, ISfpDataRepository<SfpMappingApproval> mappingApprovalRepository)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _agentCustDeliveryMapRepository = agentCustDeliveryMapRepository;
            _mappingApprovalRepository = mappingApprovalRepository;
        }

        public async Task<IEnumerable<SfpUser>> GetAllUsers()
        {
            try
            {
                return await _userRepository.GetAsync();
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetAllUsersbasedonRole(int roleId)
        {
            try
            {
                return await _userRepository.GetAsync(x=>x.RoleId == roleId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetRolebasedUsers(int roleid)
        {
            try
            {
                var results = await _userRepository.GetAsync(x=>x.RoleId == roleid);
                if (results != null && results.Count() > 0)
                    return results;
                else
                    return new List<SfpUser>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<SfpUser> GetUser(int id)
        {
            try
            {
                var results = await _userRepository.GetAsync(x => x.Id == id);
                if (results != null && results.Count() > 0)
                    return results.First();
                else
                    return new SfpUser();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetUsers(List<int?> ids)
        {
            try
            {
                var results = await _userRepository.GetAsync(x => ids.Contains(x.Id));
                if (results != null && results.Count() > 0)
                    return results;
                else
                    return new List<SfpUser>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<SfpUser> GetUserbyMobileNumber(string mobileNumber,bool isCustomer)
        {
            try
            {
                var results =  _userRepository.GetAll();
                if (results != null && results.Count() > 0)
                {
                    if (isCustomer)
                    {
                        int userId = 0;
                        // Update app installation flag...
                        var existinguserData = results.Where(x => x.Mobile == mobileNumber && x.RoleId == AppRoles.Customer);
                        if (existinguserData != null && existinguserData.Count() > 0)
                        {
                            var user = existinguserData.First();
                            if (user.IsMobileAppInstalled == null || user.IsMobileAppInstalled == false)
                            {
                                user.IsMobileAppInstalled = true;
                                await UpdateUser(user);
                                userId = user.Id;
                            }
                            else
                            {
                                return user;
                            }
                        }
                        
                        if (userId > 0)
                        {
                            var userData = await _userRepository.GetAsync(x => x.Id == userId);
                            var user = userData.First();
                            if (string.IsNullOrEmpty(user.EmailId) || string.IsNullOrEmpty(user.Mobile) || string.IsNullOrEmpty(user.RoleName) || user.RoleId == null || user.RoleId == 0)
                                user.IsProfileCompleted = false;
                            return user;
                        }
                        else
                        {
                            SfpUser user = new SfpUser();
                            user.Mobile = mobileNumber;
                            user.RoleId = AppRoles.Customer;
                            user.RoleName = "Customer";
                            user.IsProfileCompleted = false;
                            // Add IsMobileAppInstalled flag update to true
                            user.IsMobileAppInstalled = true;
                            user.Id = await CreateUser(user);
                            user = await GetUser(user.Id);
                            return user;
                        }
                    }
                    else
                    {
                        var userData = results.Where(x => x.Mobile == mobileNumber);
                        if (userData != null && userData.Count() > 0)
                        {
                            var user = userData.First();
                            if (string.IsNullOrEmpty(user.EmailId) || string.IsNullOrEmpty(user.Mobile) || string.IsNullOrEmpty(user.RoleName) || user.RoleId == null || user.RoleId == 0)
                                user.IsProfileCompleted = false;
                            return user;
                        }
                        else
                        {
                            SfpUser user = new SfpUser();
                            user.Mobile = mobileNumber;
                            user.RoleId = AppRoles.Agent;
                            user.RoleName = "Agent";
                            user.IsProfileCompleted = false;
                            user.Id = await CreateUser(user);
                            user = await GetUser(user.Id);
                            return user;
                        }
                    }
                }
                else
                {
                    SfpUser user = new SfpUser();
                    user.Mobile = mobileNumber;
                    if (isCustomer)
                    {
                        user.RoleId = AppRoles.Customer;
                        user.RoleName = "Customer";
                        user.IsMobileAppInstalled = true;
                    }
                    else
                    {
                        user.RoleId = AppRoles.Agent;
                        user.RoleName = "Agent";
                    }
                    user.IsProfileCompleted = false;
                    user.Id = await CreateUser(user);
                    user = await GetUser(user.Id);
                    return user;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<SfpUser> GetUserbyMobileNumberandRole(string mobileNumber,int roleId)
        {
            try
            {
                var results = _userRepository.GetAll();
                if (results != null && results.Count() > 0)
                {
                    var userData = results.Where(x => x.Mobile == mobileNumber && x.RoleId == roleId);
                    if (userData != null && userData.Count() > 0)
                    {
                        var user = userData.First();
                        if (string.IsNullOrEmpty(user.EmailId) || string.IsNullOrEmpty(user.Mobile) || string.IsNullOrEmpty(user.RoleName) || user.RoleId == null || user.RoleId == 0)
                            user.IsProfileCompleted = false;
                        else 
                            user.IsProfileCompleted = true;
                        return user;
                    }
                    return new SfpUser();
                }
                else
                    return new SfpUser();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> CreateUser(SfpUser user)
        {
            try
            {
                string userName = user.FirstName + " " + user.LastName;
                string roleName = user.RoleName ?? "";
                string creatorName = user.CreatorName ?? "";
                
                if((user.IsMobileAppInstalled == null || user.IsMobileAppInstalled == false) && user.RoleId == AppRoles.Customer)
                {
                    user.ApprovalStatus = "Approved";
                }                
                await _userRepository.CreateAsync(user);

                // Role Creation..
                SfpUserRoleMap sfpUserRoleMap = new SfpUserRoleMap();
                sfpUserRoleMap.UserId = user.Id;
                sfpUserRoleMap.RoleId = user.RoleId ?? 0;
                sfpUserRoleMap.CreatedBy = user.CreatedBy ?? 0;
                sfpUserRoleMap.CreatedOn = DateTime.Now;
                await _userRoleRepository.CreateAsync(sfpUserRoleMap);

                // Agent -- Delivery Boy Mapping -- Adding Customer tempararily
                if (user.RoleId == AppRoles.Delivery)
                {
                    SfpAgentCustDeliveryMap agentCustDeliveryMap = new SfpAgentCustDeliveryMap();
                    agentCustDeliveryMap.AgentId = user.CreatedBy;
                    agentCustDeliveryMap.AgentName = creatorName;
                    agentCustDeliveryMap.DeliveryId = user.Id;
                    agentCustDeliveryMap.DeliveryName = userName;
                    agentCustDeliveryMap.CreatedBy = user.CreatedBy;
                    agentCustDeliveryMap.CreatorName = creatorName;
                    await _agentCustDeliveryMapRepository.CreateAsync(agentCustDeliveryMap);
                }
                //else if (user.RoleId == AppRoles.Customer)
                //{
                //    SfpAgentCustDeliveryMap agentCustDeliveryMap = new SfpAgentCustDeliveryMap();
                //    agentCustDeliveryMap.AgentId = user.CreatedBy;
                //    agentCustDeliveryMap.AgentName = creatorName;
                //    agentCustDeliveryMap.CustomerId = user.Id;
                //    agentCustDeliveryMap.CustomerName = userName;
                //    agentCustDeliveryMap.CreatedBy = user.CreatedBy;
                //    agentCustDeliveryMap.CreatorName = creatorName;
                //    await _agentCustDeliveryMapRepository.CreateAsync(agentCustDeliveryMap);
                //}
                // Need to un comment this block...
                else if (user.RoleId == AppRoles.Customer && (user.CreatedBy != null && user.CreatedBy > 0))
                {
                    if (user.IsMobileAppInstalled == null || user.IsMobileAppInstalled == false)
                    {
                        SfpAgentCustDeliveryMap agentCustDeliveryMap = new SfpAgentCustDeliveryMap();
                        agentCustDeliveryMap.AgentId = user.CreatedBy;
                        agentCustDeliveryMap.AgentName = creatorName;
                        agentCustDeliveryMap.CustomerId = user.Id;
                        agentCustDeliveryMap.CustomerName = userName;
                        agentCustDeliveryMap.CreatedBy = user.CreatedBy;
                        agentCustDeliveryMap.CreatedOn = DateTime.Now;
                        // Add User
                        return await _agentCustDeliveryMapRepository.CreateAsync(agentCustDeliveryMap);
                    }
                    else
                    {
                        // fill the approal details with vedor...
                        var insertedUserData = await _userRepository.GetAsync(x => x.Id == user.CreatedBy);

                        SfpMappingApproval sfpMappingApproval = new SfpMappingApproval();
                        sfpMappingApproval.CustomerName = insertedUserData.First().FirstName + " " + insertedUserData.First().LastName;
                        sfpMappingApproval.CustomerEmailId = insertedUserData.First().EmailId;
                        sfpMappingApproval.CustomerMobile = insertedUserData.First().Mobile;
                        sfpMappingApproval.CustomerAltMobile = insertedUserData.First().AltMobile;
                        sfpMappingApproval.StartDate = insertedUserData.First().StartDate;
                        sfpMappingApproval.StateName = insertedUserData.First().StateName;
                        sfpMappingApproval.CityName = insertedUserData.First().CityName;
                        sfpMappingApproval.Address = insertedUserData.First().Address;
                        sfpMappingApproval.LandMark = insertedUserData.First().LandMark;
                        sfpMappingApproval.PinCode = insertedUserData.First().PinCode;
                        sfpMappingApproval.CustomerId = user.Id;
                        //sfpMappingApproval.CustomerName = userName;
                        sfpMappingApproval.AgentName = creatorName;
                        sfpMappingApproval.AgentId = user.CreatedBy;
                        sfpMappingApproval.ActionStatus = "Pending";
                        sfpMappingApproval.IsForVendorApproval = false;
                        await _mappingApprovalRepository.CreateAsync(sfpMappingApproval);
                    }
                }
                return user.Id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateUser(SfpUser user)
        {
            try
            {
                await _userRepository.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteUser(int id)
        {
            try
            {
                var user = await _userRepository.GetAsync(x => x.Id == id);
                if(user!=null && user.Count() > 0)
                {
                    await _userRepository.DeleteAsync(user.First());
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}