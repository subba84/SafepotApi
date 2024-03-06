using Microsoft.VisualBasic;
using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpAgentCustDeliveryMapService : ISfpAgentCustDeliveryMapService
    {
        private readonly ISfpDataRepository<SfpAgentCustDeliveryMap> _sfpMappingRepository;
        private readonly ISfpDataRepository<SfpUser> _sfpUserRepository;
        private readonly ISfpDataRepository<SfpMappingApproval> _sfpMappingApprovalRepository;
        private readonly ISfpDataRepository<SfpUser> _userRepository;
        public SfpAgentCustDeliveryMapService(ISfpDataRepository<SfpAgentCustDeliveryMap> sfpMappingRepository,
            ISfpDataRepository<SfpUser> sfpUserRepository,
            ISfpDataRepository<SfpMappingApproval> sfpMappingApprovalRepository,
            ISfpDataRepository<SfpUser> userRepository)
        {
            _sfpMappingRepository = sfpMappingRepository;
            _sfpUserRepository = sfpUserRepository;
            _sfpMappingApprovalRepository = sfpMappingApprovalRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<SfpUser>> GetAgentAssociatedCustomers(int agentid)
        {
            try
            {
                List<SfpUser> usersList = new List<SfpUser>();
                List<int?> ids = new List<int?>();
                var mapdata = await _sfpMappingRepository.GetAsync(x=>x.AgentId == agentid);
                if(mapdata != null && mapdata.Count() > 0)
                {
                    ids = mapdata.Select(x => x.CustomerId).Distinct().ToList();
                    var users = await _sfpUserRepository.GetAsync(x=> ids.Contains(x.Id));
                    if(users != null && users.Count() > 0)
                    {
                        usersList.AddRange(users.ToList());
                    }
                    usersList.ForEach(x =>
                    {
                        x.ApprovalStatus = "Approved";
                    });
                }
                var pendingCustomers = await _sfpMappingApprovalRepository.GetAsync(x => x.AgentId == agentid && x.IsForVendorApproval == false/* && x.ActionStatus == "Pending"*/);
                if (pendingCustomers.Where(x => x.ActionStatus == "Pending") != null && pendingCustomers.Where(x => x.ActionStatus == "Pending").Count() > 0)
                {
                    ids = pendingCustomers.Where(x => x.ActionStatus == "Pending").Select(x => x.CustomerId).Distinct().ToList();
                    var users = await _sfpUserRepository.GetAsync(x => ids.Contains(x.Id));
                    if (users != null && users.Count() > 0)
                    {
                        var modusers = users.ToList();
                        modusers.ForEach(x => {
                            x.ApprovalStatus = "Pending";
                        });
                        usersList.AddRange(modusers);
                    }
                }
                return usersList;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetAgentAssociatedDeliveryBoys(int agentid)
        {
            try
            {
                var mapdata = await _sfpMappingRepository.GetAsync(x => x.AgentId == agentid);
                if (mapdata != null && mapdata.Count() > 0)
                {
                    List<int?> ids = mapdata.Select(x => x.DeliveryId).Distinct().ToList();
                    return await _sfpUserRepository.GetAsync(x => ids.Contains(x.Id));
                }
                return new List<SfpUser>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetFreeCustomersbasedonAgentandDelivery(int agentId,int deliveryId)
        {
            //try
            //{
            //    List<int?> customerids = new List<int?>();
            //    List<int?> pendingCustomers = new List<int?>();
            //    var mappingApprovalPendingCustomers = await _sfpMappingApprovalRepository.GetAsync(x => x.AgentId == agentId);
            //    if (mappingApprovalPendingCustomers != null && mappingApprovalPendingCustomers.Count() > 0)
            //    {
            //        var pendingCustomerIds = mappingApprovalPendingCustomers.Where(x => x.ActionStatus == "Pending");
            //        if (pendingCustomerIds != null && pendingCustomerIds.Count() > 0)
            //        {
            //            pendingCustomers.AddRange(pendingCustomerIds.Select(x => x.CustomerId).Distinct().ToList());
            //        }
            //    }
            //    var data = await GetAllMappingsbasedonAgent(agentId);
            //    if (data != null && data.Count() > 0)
            //    {
            //        var distinctcustomers = await _sfpUserRepository.GetAsync(x => x.RoleId == AppRoles.Customer);// data.Select(x => x.AgentId).Distinct().ToList();
            //        if (distinctcustomers != null && distinctcustomers.Count() > 0)
            //        {
            //            foreach (var customer in distinctcustomers)
            //            {
            //                var custdeliverymapdata = data.Where(x => x.CustomerId == customer.Id && x.CustomerId == agentId);
            //                if (custdeliverymapdata == null || custdeliverymapdata.Count() == 0)
            //                {
            //                    customerids.Add(Convert.ToInt32(customer.Id));
            //                }
            //            }
            //        }
            //        // Filter out approval pending customers
            //        customerids = customerids.Where(x => !pendingCustomers.Contains(x)).ToList();
            //        return await _sfpUserRepository.GetAsync(x => customerids.Contains(x.Id));
            //    }
            //    else
            //    {
            //        var customers = await _sfpUserRepository.GetAsync(x => x.RoleId == AppRoles.Customer);
            //        return customers.Where(x => !pendingCustomers.Contains(x.Id));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}


            try
            {
                List<int> customerids = new List<int>();
                var data = await GetAllMappingsbasedonAgent(agentId);
                if (data != null && data.Count() > 0)
                {
                    var distinctcustomers = data.Select(x => x.CustomerId).Distinct().ToList();
                    if(distinctcustomers!=null && distinctcustomers.Count() > 0)
                    {
                        foreach(var customer in distinctcustomers)
                        {
                            var custdeliverymapdata = data.Where(x => x.CustomerId == customer && x.DeliveryId == deliveryId);
                            if(custdeliverymapdata == null || custdeliverymapdata.Count() == 0)
                            {
                                customerids.Add(Convert.ToInt32(customer));
                            }
                        }
                    }
                }
                //return await _sfpUserRepository.GetAsync(x => customerids.Contains(x.Id)); ;
                var users = await _sfpUserRepository.GetAsync(x => customerids.Contains(x.Id));
                List<SfpUser> usersList = new List<SfpUser>();
                if (users != null && users.Count() > 0)
                {
                    usersList = users.ToList();
                    usersList.ForEach(x =>
                    {
                        x.ApprovalStatus = "Approved";
                    });
                }
                return usersList;



            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetFreeAgentsbasedoCustomer(int customerId)
        {
            try
            {
                List<int?> agentids = new List<int?>();
                List<int?> pendingAgents = new List<int?>();
                var mappingApprovalPendingAgents = await _sfpMappingApprovalRepository.GetAsync(x => x.CustomerId == customerId);
                if (mappingApprovalPendingAgents != null && mappingApprovalPendingAgents.Count() > 0)
                {
                    var pendingAgentIds = mappingApprovalPendingAgents.Where(x => x.ActionStatus == "Pending");
                    if (pendingAgentIds != null && pendingAgentIds.Count() > 0)
                    {
                        pendingAgents.AddRange(pendingAgentIds.Select(x => x.AgentId).Distinct().ToList());
                    }
                }
                var data = await GetAllMappingsbasedonCustomer(customerId);
                if (data != null && data.Count() > 0)
                {
                    var distinctagents = await _sfpUserRepository.GetAsync(x => x.RoleId == AppRoles.Agent);// data.Select(x => x.AgentId).Distinct().ToList();
                    if (distinctagents != null && distinctagents.Count() > 0)
                    {
                        foreach (var agent in distinctagents)
                        {
                            var custdeliverymapdata = data.Where(x => x.AgentId == agent.Id && x.CustomerId == customerId);
                            if (custdeliverymapdata == null || custdeliverymapdata.Count() == 0)
                            {
                                agentids.Add(Convert.ToInt32(agent.Id));
                            }
                        }
                    }
                    // Filter out approval pending agents
                    agentids = agentids.Where(x => !pendingAgents.Contains(x)).ToList();
                    return await _sfpUserRepository.GetAsync(x => agentids.Contains(x.Id));
                }
                else
                {
                    var agents = await _sfpUserRepository.GetAsync(x => x.RoleId == AppRoles.Agent);
                    return agents.Where(x => !pendingAgents.Contains(x.Id));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetCustomerAssociatedDeliveryBoys(int customerid)
        {
            try
            {
                var mapdata = await _sfpMappingRepository.GetAsync(x => x.CustomerId == customerid);
                if (mapdata != null && mapdata.Count() > 0)
                {
                    List<int?> ids = mapdata.Select(x => x.DeliveryId).Distinct().ToList();
                    var users = await _sfpUserRepository.GetAsync(x => ids.Contains(x.Id));
                    List<SfpUser> usersList = new List<SfpUser>();
                    if (users != null && users.Count() > 0)
                    {
                        usersList = users.ToList();
                        usersList.ForEach(x =>
                        {
                            x.ApprovalStatus = "Approved";
                        });
                    }
                    return usersList;
                }
                return new List<SfpUser>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetCustomerAssociatedAgents(int customerid)
        {
            try
            {
                List<SfpUser> users = new List<SfpUser>();
                List<int?> pendingAgents = new List<int?>();
                var mappingApprovalPendingAgents = await _sfpMappingApprovalRepository.GetAsync(x => x.CustomerId == customerid);
                if (mappingApprovalPendingAgents != null && mappingApprovalPendingAgents.Count() > 0)
                {
                    var pendingAgentIds = mappingApprovalPendingAgents.Where(x => x.ActionStatus == "Pending" && x.IsForVendorApproval == true);
                    if (pendingAgentIds != null && pendingAgentIds.Count() > 0)
                    {
                        pendingAgents.AddRange(pendingAgentIds.Select(x => x.AgentId).Distinct().ToList());
                        var pendingusers = await _sfpUserRepository.GetAsync(x => pendingAgents.Contains(x.Id));
                        users = pendingusers.ToList();
                        users.ForEach(x =>
                        {
                            x.ApprovalStatus = "Pending";
                        });
                    }
                }
                var mapdata = await _sfpMappingRepository.GetAsync(x => x.CustomerId == customerid);
                if (mapdata != null && mapdata.Count() > 0)
                {
                    List<int?> ids = mapdata.Select(x => x.AgentId).Distinct().ToList();
                    var mappedusers = await _sfpUserRepository.GetAsync(x => ids.Contains(x.Id));
                    List<SfpUser> userslist = mappedusers.ToList();
                    userslist.ForEach(x =>
                    {
                        x.ApprovalStatus = "Approved";
                    });
                    users.AddRange(userslist);
                }
                return users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetDeliveryAssociatedAgents(int deliveryid)
        {
            try
            {
                var mapdata = await _sfpMappingRepository.GetAsync(x => x.DeliveryId == deliveryid);
                if (mapdata != null && mapdata.Count() > 0)
                {
                    List<int?> ids = mapdata.Select(x => x.AgentId).Distinct().ToList();
                    var users = await _sfpUserRepository.GetAsync(x => ids.Contains(x.Id));
                    List<SfpUser> usersList = new List<SfpUser>();
                    if (users != null && users.Count() > 0)
                    {
                        usersList = users.ToList();
                        usersList.ForEach(x =>
                        {
                            x.ApprovalStatus = "Approved";
                        });
                    }
                    return usersList;
                }
                return new List<SfpUser>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetDeliveryAssociatedCustomers(int deliveryid)
        {
            try
            {
                var mapdata = await _sfpMappingRepository.GetAsync(x => x.DeliveryId == deliveryid);
                if (mapdata != null && mapdata.Count() > 0)
                {
                    List<int?> ids = mapdata.Select(x => x.CustomerId).Distinct().ToList();
                    var users = await _sfpUserRepository.GetAsync(x => ids.Contains(x.Id));
                    List<SfpUser> usersList = new List<SfpUser>();
                    if(users!=null && users.Count() > 0)
                    {
                        usersList = users.ToList();
                        usersList.ForEach(x =>
                        {
                            x.ApprovalStatus = "Approved";
                        });
                    }
                    return usersList;
                }
                return new List<SfpUser>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpAgentCustDeliveryMap>> GetAllMappings()
        {
            try
            {
                return await _sfpMappingRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpAgentCustDeliveryMap>> GetAllMappingsbasedonCustomer(int customerId)
        {
            try
            {
                return await _sfpMappingRepository.GetAsync(x=>x.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpAgentCustDeliveryMap>> GetAllMappingsbasedonAgent(int agentId)
        {
            try
            {
                return await _sfpMappingRepository.GetAsync(x=>x.AgentId == agentId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpAgentCustDeliveryMap> GetMapping(int id)
        {
            try
            {
                var data = await _sfpMappingRepository.GetAsync(x=>x.Id == id);
                if (data != null && data.Count() > 0)
                    return data.First();
                return new SfpAgentCustDeliveryMap();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> SaveMapping(SfpAgentCustDeliveryMap sfpAgentCustDeliveryMap)
        {
            try
            {
                if((sfpAgentCustDeliveryMap.AgentId != null && sfpAgentCustDeliveryMap.AgentId > 0) && (sfpAgentCustDeliveryMap.CustomerId != null && sfpAgentCustDeliveryMap.CustomerId > 0) && (sfpAgentCustDeliveryMap.DeliveryId == 0))
                {
                    // Fill the details with Customer....
                    var insertedUserData = await _userRepository.GetAsync(x => x.Id == sfpAgentCustDeliveryMap.CustomerId);
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
                    sfpMappingApproval.CustomerId = sfpAgentCustDeliveryMap.CustomerId;
                    //sfpMappingApproval.CustomerName = userName;
                    sfpMappingApproval.AgentName = string.Empty;
                    sfpMappingApproval.AgentId = sfpAgentCustDeliveryMap.AgentId;
                    sfpMappingApproval.ActionStatus = "Pending";
                    sfpMappingApproval.IsForVendorApproval = true;
                    return await _sfpMappingApprovalRepository.CreateAsync(sfpMappingApproval);
                }
                else
                {
                    return await _sfpMappingRepository.CreateAsync(sfpAgentCustDeliveryMap);
                }
                //return await _sfpMappingRepository.CreateAsync(sfpAgentCustDeliveryMap);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateMapping(SfpAgentCustDeliveryMap sfpAgentCustDeliveryMap)
        {
            try
            {
                await _sfpMappingRepository.UpdateAsync(sfpAgentCustDeliveryMap);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteMapping(SfpAgentCustDeliveryMap sfpAgentCustDeliveryMap)
        {
            try
            {
                await _sfpMappingRepository.DeleteAsync(sfpAgentCustDeliveryMap);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpUser>> GetAssociatedCustomersbasedonAgentandDelivery(int agentId, int deliveryId)
        {
            try
            {
                List<int> customerids = new List<int>();
                var data = await GetAllMappingsbasedonAgent(agentId);
                if (data != null && data.Count() > 0)
                {
                    var distinctcustomers = data.Where(x=>x.CustomerId!=null).Select(x => x.CustomerId).Distinct().ToList();
                    if (distinctcustomers != null && distinctcustomers.Count() > 0)
                    {
                        foreach (var customer in distinctcustomers)
                        {
                            var custdeliverymapdata = data.Where(x => x.CustomerId == customer && x.DeliveryId == deliveryId);
                            if (custdeliverymapdata != null && custdeliverymapdata.Count() > 0)
                            {
                                customerids.Add(Convert.ToInt32(customer));
                            }
                        }
                    }
                }
                var users = await _sfpUserRepository.GetAsync(x => customerids.Contains(x.Id));
                return users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
