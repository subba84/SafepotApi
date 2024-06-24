using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpMappingApprovalService : ISfpMappingApprovalService
    {
        private readonly ISfpDataRepository<SfpMappingApproval> _sfpMappingApprovalRepository;
        private readonly ISfpDataRepository<SfpAgentCustDeliveryMap> _agentCustMappingRepository;
        private readonly INotificationService _notificationService;
        private readonly ISfpDataRepository<SfpUser> _userRepository;
        public SfpMappingApprovalService(ISfpDataRepository<SfpMappingApproval> sfpMappingApprovalRepository,
            ISfpDataRepository<SfpAgentCustDeliveryMap> agentCustMappingRepository,
            ISfpDataRepository<SfpUser> userRepository,
            INotificationService notificationService)
        {
            _sfpMappingApprovalRepository = sfpMappingApprovalRepository;
            _agentCustMappingRepository = agentCustMappingRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }       

        public async Task<IEnumerable<SfpMappingApproval>> GetMappingApprovalsbasedonAgent(int agentId)
        {
            try
            {
                return await _sfpMappingApprovalRepository.GetAsync(x=>x.IsForVendorApproval == true && x.AgentId == agentId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpMappingApproval>> GetMappingApprovalsbasedonAgentandStatus(int agentId,string status)
        {
            try
            {
                var data = await _sfpMappingApprovalRepository.GetAsync(x => x.IsForVendorApproval == true && x.AgentId == agentId);
                if(data!= null && data.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        data = data.Where(x => x.ActionStatus == status);
                        if(data!=null && data.Count() > 0)
                        {
                            var customers = data.Select(x => x.CustomerId).Distinct().ToList();
                            var users = await _userRepository.GetAsync(x => customers.Contains(x.Id));
                            foreach (var sfpMappingApproval in data)
                            {
                                var cust = users.Where(x => x.Id == sfpMappingApproval.CustomerId).First();
                                sfpMappingApproval.CustomerName = cust.FirstName + " " + cust.LastName;
                                sfpMappingApproval.CustomerEmailId = cust.EmailId;
                                sfpMappingApproval.CustomerMobile = cust.Mobile;
                                sfpMappingApproval.CustomerAltMobile = cust.AltMobile;
                                sfpMappingApproval.StartDate = cust.StartDate;
                                sfpMappingApproval.StateName = cust.StateName;
                                sfpMappingApproval.CityName = cust.CityName;
                                sfpMappingApproval.Address = cust.Address;
                                sfpMappingApproval.LandMark = cust.LandMark;
                                sfpMappingApproval.PinCode = cust.PinCode;
                            }
                        }
                        return data ?? new List<SfpMappingApproval>();
                    }
                }
                return new List<SfpMappingApproval>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpMappingApproval>> GetMappingApprovalsbasedonCustomerandStatus(int customerId, string status)
        {
            try
            {
                var data = await _sfpMappingApprovalRepository.GetAsync(x => x.IsForVendorApproval == false && x.CustomerId == customerId);
                if (data != null && data.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        data = data.Where(x => x.ActionStatus == status);
                        if (data != null && data.Count() > 0)
                        {
                            var agents = data.Select(x => x.AgentId).Distinct().ToList();
                            var users = await _userRepository.GetAsync(x => agents.Contains(x.Id));
                            foreach (var sfpMappingApproval in data)
                            {
                                var cust = users.Where(x => x.Id == sfpMappingApproval.AgentId).First();
                                sfpMappingApproval.CustomerName = cust.FirstName + " " + cust.LastName;
                                sfpMappingApproval.CustomerEmailId = cust.EmailId;
                                sfpMappingApproval.CustomerMobile = cust.Mobile;
                                sfpMappingApproval.CustomerAltMobile = cust.AltMobile;
                                sfpMappingApproval.StartDate = cust.StartDate;
                                sfpMappingApproval.StateName = cust.StateName;
                                sfpMappingApproval.CityName = cust.CityName;
                                sfpMappingApproval.Address = cust.Address;
                                sfpMappingApproval.LandMark = cust.LandMark;
                                sfpMappingApproval.PinCode = cust.PinCode;
                            }
                        }
                        return data ?? new List<SfpMappingApproval>();
                    }
                }
                return new List<SfpMappingApproval>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpMappingApproval>> GetMappingApprovalsbasedonCustomer(int customerId)
        {
            try
            {
                return await _sfpMappingApprovalRepository.GetAsync(x => x.IsForVendorApproval == false && x.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveMappingApproval(SfpMappingApproval sfpMappingApproval)
        {
            try
            {
                if (sfpMappingApproval.CustomerId != null && sfpMappingApproval.CustomerId != 0)
                {                    
                    if (sfpMappingApproval.IsForVendorApproval == true)
                    {
                        var customer = await _userRepository.GetAsync(x => x.Id == sfpMappingApproval.CustomerId);
                        if (customer != null)
                        {
                            var cust = customer.First();                            
                            sfpMappingApproval.CustomerName = cust.FirstName + " " + cust.LastName;
                            sfpMappingApproval.CustomerEmailId = cust.EmailId;
                            sfpMappingApproval.CustomerMobile = cust.Mobile;
                            sfpMappingApproval.CustomerAltMobile = cust.AltMobile;
                            sfpMappingApproval.StartDate = cust.StartDate;
                            sfpMappingApproval.StateName = cust.StateName;
                            sfpMappingApproval.CityName = cust.CityName;
                            sfpMappingApproval.Address = cust.Address;
                            sfpMappingApproval.LandMark = cust.LandMark;
                            sfpMappingApproval.PinCode = cust.PinCode;
                        }                        
                    }
                    else
                    {
                        var agent = await _userRepository.GetAsync(x => x.Id == sfpMappingApproval.AgentId);
                        if (agent != null)
                        {
                            var agnt = agent.First();
                            sfpMappingApproval.CustomerName = agnt.FirstName + " " + agnt.LastName;
                            sfpMappingApproval.CustomerEmailId = agnt.EmailId;
                            sfpMappingApproval.CustomerMobile = agnt.Mobile;
                            sfpMappingApproval.CustomerAltMobile = agnt.AltMobile;
                            sfpMappingApproval.StartDate = agnt.StartDate;
                            sfpMappingApproval.StateName = agnt.StateName;
                            sfpMappingApproval.CityName = agnt.CityName;
                            sfpMappingApproval.Address = agnt.Address;
                            sfpMappingApproval.LandMark = agnt.LandMark;
                            sfpMappingApproval.PinCode = agnt.PinCode;
                        }
                    }
                }
                await _sfpMappingApprovalRepository.CreateAsync(sfpMappingApproval);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateMappingApproval(SfpMappingApproval sfpMappingApproval)
        {
            try
            {
                sfpMappingApproval.ActionPerformedOn = DateTime.Now;
                if (sfpMappingApproval.CustomerId != null && sfpMappingApproval.CustomerId != 0)
                {
                    if(sfpMappingApproval.IsForVendorApproval == true)
                    {
                        var customer = await _userRepository.GetAsync(x => x.Id == sfpMappingApproval.CustomerId);
                        if (customer != null)
                        {
                            var cust = customer.First();
                            sfpMappingApproval.CustomerName = cust.FirstName + " " + cust.LastName;
                            sfpMappingApproval.CustomerEmailId = cust.EmailId;
                            sfpMappingApproval.CustomerMobile = cust.Mobile;
                            sfpMappingApproval.CustomerAltMobile = cust.AltMobile;
                            sfpMappingApproval.StartDate = cust.StartDate;
                            sfpMappingApproval.StateName = cust.StateName;
                            sfpMappingApproval.CityName = cust.CityName;
                            sfpMappingApproval.Address = cust.Address;
                            sfpMappingApproval.LandMark = cust.LandMark;
                            sfpMappingApproval.PinCode = cust.PinCode;
                        }
                    }
                    else
                    {
                        var agent = await _userRepository.GetAsync(x => x.Id == sfpMappingApproval.AgentId);
                        if (agent != null)
                        {
                            var agnt = agent.First();
                            sfpMappingApproval.CustomerName = agnt.FirstName + " " + agnt.LastName;
                            sfpMappingApproval.CustomerEmailId = agnt.EmailId;
                            sfpMappingApproval.CustomerMobile = agnt.Mobile;
                            sfpMappingApproval.CustomerAltMobile = agnt.AltMobile;
                            sfpMappingApproval.StartDate = agnt.StartDate;
                            sfpMappingApproval.StateName = agnt.StateName;
                            sfpMappingApproval.CityName = agnt.CityName;
                            sfpMappingApproval.Address = agnt.Address;
                            sfpMappingApproval.LandMark = agnt.LandMark;
                            sfpMappingApproval.PinCode = agnt.PinCode;
                        }
                    }
                   
                }
                // Create Mapping of Agent and Customer if status is approved...
                if (sfpMappingApproval.ActionStatus == "Approved")
                {
                    SfpAgentCustDeliveryMap agentCustDeliveryMap = new SfpAgentCustDeliveryMap();
                    agentCustDeliveryMap.AgentId = sfpMappingApproval.AgentId;
                    agentCustDeliveryMap.AgentName = sfpMappingApproval.AgentName;
                    agentCustDeliveryMap.CustomerId = sfpMappingApproval.CustomerId;
                    agentCustDeliveryMap.CustomerName = sfpMappingApproval.CustomerName;
                    agentCustDeliveryMap.CreatedBy = sfpMappingApproval.ActionPerformedBy;
                    agentCustDeliveryMap.CreatedOn = sfpMappingApproval.ActionPerformedOn;
                    string customerdescription = String.Empty;string agentdescription = string.Empty;
                    customerdescription = "You have been assigned to Vendor - " + sfpMappingApproval.AgentName + " as a customer on " + DateTime.Now.ToString("dd-MM-yyyy hh:mm");
                    agentdescription = sfpMappingApproval.CustomerName + " have been assigned to you as a customer on " + DateTime.Now.ToString("dd-MM-yyyy hh:mm");
                    
                    //Notification creation
                    await _notificationService.CreateNotification(customerdescription, sfpMappingApproval.AgentId, sfpMappingApproval.CustomerId, null, null,"Assigning User",false,true,false);
                    await _notificationService.CreateNotification(agentdescription, sfpMappingApproval.AgentId, sfpMappingApproval.CustomerId, null, null, "Assigning User", true, false, false);

                    // Add User
                    await _agentCustMappingRepository.CreateAsync(agentCustDeliveryMap);

                }
                await _sfpMappingApprovalRepository.UpdateAsync(sfpMappingApproval);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
