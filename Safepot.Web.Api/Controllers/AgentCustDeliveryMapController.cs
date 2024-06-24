using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using NPOI.POIFS.Crypt.Dsig;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AgentCustDeliveryMapController : ControllerBase
    {
        private readonly ISfpAgentCustDeliveryMapService _sfpAgentCustDeliveryMapService;
        private readonly ILogger<AgentCustDeliveryMapController> _logger;
        private readonly INotificationService _notificationService;
        private readonly ISfpUserService _userService;
        public AgentCustDeliveryMapController(ISfpAgentCustDeliveryMapService sfpAgentCustDeliveryMapService,
            ILogger<AgentCustDeliveryMapController> logger,
            INotificationService notificationService,
            ISfpUserService userService)
        {
            _sfpAgentCustDeliveryMapService = sfpAgentCustDeliveryMapService;
            _logger = logger;
            _notificationService = notificationService;
            _userService = userService;
        }

        [HttpGet]
        [Route("getagentcustomerdeliverymappings")]
        public async Task<ResponseModel<SfpAgentCustDeliveryMap>> GetMappings()
        {
            try
            {
                var data = await _sfpAgentCustDeliveryMapService.GetAllMappings();
                return ResponseModel<SfpAgentCustDeliveryMap>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpAgentCustDeliveryMap>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getcustomersbasedonagent/{agentid}")]
        public async Task<ResponseModel<SfpUser>> GetCustomersbasedOnAgent(int agentid)
        {
            try
            {
                var data = await _sfpAgentCustDeliveryMapService.GetAgentAssociatedCustomers(agentid);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }


        [HttpGet]
        [Route("getfreecustomersbasedonagentanddeliveryboy")]
        public async Task<ResponseModel<SfpUser>> GetFreeCustomersbasedOnAgentandDeliveryBoy(int agentid,int deliveryid)
        {
            try
            {
                var data = await _sfpAgentCustDeliveryMapService.GetFreeCustomersbasedonAgentandDelivery(agentid, deliveryid);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getfreeagentsbasedoncustomer")]
        public async Task<ResponseModel<SfpUser>> GetFreeAgentsbasedOnCustomer(int customerId)
        {
            try
            {
                var data = await _sfpAgentCustDeliveryMapService.GetFreeAgentsbasedoCustomer(customerId);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getdeliverysbasedonagent/{agentid}")]
        public async Task<ResponseModel<SfpUser>> GetDeliverysbasedOnAgent(int agentid)
        {
            try
            {
                var data = await _sfpAgentCustDeliveryMapService.GetAgentAssociatedDeliveryBoys(agentid);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getcustomersbasedondelivery/{deliveryid}")]
        public async Task<ResponseModel<SfpUser>> GetCustomersbasedOnDelivery(int deliveryid)
        {
            try
            {
                var data = await _sfpAgentCustDeliveryMapService.GetDeliveryAssociatedCustomers(deliveryid);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getagentsbasedondelivery/{deliveryid}")]
        public async Task<ResponseModel<SfpUser>> GetAgentsbasedOnDelivery(int deliveryid)
        {
            try
            {
                var data = await _sfpAgentCustDeliveryMapService.GetDeliveryAssociatedAgents(deliveryid);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getagensbasedoncustomer/{customerid}")]
        public async Task<ResponseModel<SfpUser>> GetAgentsbasedOnCustomer(int customerid)
        {
            try
            {
                var data = await _sfpAgentCustDeliveryMapService.GetCustomerAssociatedAgents(customerid);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getdeliverysbasedoncustomer/{customerid}")]
        public async Task<ResponseModel<SfpUser>> GetDeliverysbasedOnCustomer(int customerid)
        {
            try
            {
                var data = await _sfpAgentCustDeliveryMapService.GetCustomerAssociatedDeliveryBoys(customerid);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }



        [HttpPost]
        [Route("saveagentcustdeliverymapping")]
        public async Task<ResponseModel<SfpAgentCustDeliveryMap>> SaveMapping([FromBody] SfpAgentCustDeliveryMap sfpAgentCustDeliveryMap)
        {
            try
            {
                if (sfpAgentCustDeliveryMap.DeliveryId != 0)
                {
                    List<int?> usersList = new List<int?> { sfpAgentCustDeliveryMap.AgentId, sfpAgentCustDeliveryMap.CustomerId, sfpAgentCustDeliveryMap.DeliveryId };
                    var userDetails = await _userService.GetUsers(usersList);
                    if (userDetails != null && userDetails.Count() > 0)
                    {
                        var deliveryBoy = userDetails.First(x => x.Id == sfpAgentCustDeliveryMap.DeliveryId);
                        var customer = userDetails.First(x => x.Id == sfpAgentCustDeliveryMap.CustomerId);
                        var agent = userDetails.First(x => x.Id == sfpAgentCustDeliveryMap.AgentId);
                        string customerDescription = "Delivery Boy - " + deliveryBoy.FirstName + " " + deliveryBoy.LastName + " have been assigned to you";
                        string deliveryDescription = "you have been assigned to Customer - " + customer.FirstName + " " + customer.LastName;
                        string agentDescription = "Delivery Boy - " + deliveryBoy.FirstName + " " + deliveryBoy.LastName + " have been assigned to Customer - " + customer.FirstName + " " + customer.LastName;

                        await _notificationService.CreateNotification(deliveryDescription, sfpAgentCustDeliveryMap.AgentId, sfpAgentCustDeliveryMap.CustomerId, sfpAgentCustDeliveryMap.DeliveryId, null, "Assigning User", false, false, true);
                        await _notificationService.CreateNotification(customerDescription, sfpAgentCustDeliveryMap.AgentId, sfpAgentCustDeliveryMap.CustomerId, sfpAgentCustDeliveryMap.DeliveryId, null, "Assigning User", false, true, false);
                        await _notificationService.CreateNotification(agentDescription, sfpAgentCustDeliveryMap.AgentId, sfpAgentCustDeliveryMap.CustomerId, sfpAgentCustDeliveryMap.DeliveryId, null, "Assigning User", true, false, false);
                    }
                }
                await _sfpAgentCustDeliveryMapService.SaveMapping(sfpAgentCustDeliveryMap);
                
                return ResponseModel<SfpAgentCustDeliveryMap>.ToApiResponse("Success", "Mapping Save Successful", new List<SfpAgentCustDeliveryMap>() { new SfpAgentCustDeliveryMap { Id = sfpAgentCustDeliveryMap.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpAgentCustDeliveryMap>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updateagentcustdeliverymapping")]
        public async Task<ResponseModel<SfpAgentCustDeliveryMap>> UpdateMapping([FromBody] SfpAgentCustDeliveryMap sfpAgentCustDeliveryMap)
        {
            try
            {
                await _sfpAgentCustDeliveryMapService.UpdateMapping(sfpAgentCustDeliveryMap);
                return ResponseModel<SfpAgentCustDeliveryMap>.ToApiResponse("Success", "Mapping Update Successful", new List<SfpAgentCustDeliveryMap>() { new SfpAgentCustDeliveryMap { Id = sfpAgentCustDeliveryMap.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpAgentCustDeliveryMap>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deleteagentcustdeliverymapping")]
        public async Task<ResponseModel<SfpAgentCustDeliveryMap>> DeleteMapping(int id)
        {
            try
            {
                var data = await _sfpAgentCustDeliveryMapService.GetMapping(id);
                await _sfpAgentCustDeliveryMapService.DeleteMapping(data);
                return ResponseModel<SfpAgentCustDeliveryMap>.ToApiResponse("Success", "Mapping Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpAgentCustDeliveryMap>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
