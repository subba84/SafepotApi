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
    [Route("api/[controller]")]
    [ApiController]
    public class AgentCustDeliveryMapController : ControllerBase
    {
        private readonly ISfpAgentCustDeliveryMapService _sfpAgentCustDeliveryMapService;
        private readonly ILogger<AgentCustDeliveryMapController> _logger;
        public AgentCustDeliveryMapController(ISfpAgentCustDeliveryMapService sfpAgentCustDeliveryMapService, ILogger<AgentCustDeliveryMapController> logger)
        {
            _sfpAgentCustDeliveryMapService = sfpAgentCustDeliveryMapService;
            _logger = logger;
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
