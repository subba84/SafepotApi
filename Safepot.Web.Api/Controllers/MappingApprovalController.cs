using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Safepot.Business;
using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MappingApprovalController : ControllerBase
    {
        private readonly ISfpMappingApprovalService _sfpMappingApprovalService;
        private readonly ILogger<MappingApprovalController> _logger;
        public MappingApprovalController(ISfpMappingApprovalService sfpMappingApprovalService, ILogger<MappingApprovalController> logger)
        {
            _sfpMappingApprovalService = sfpMappingApprovalService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getapprovals/{agentId}")]
        public async Task<ResponseModel<SfpMappingApproval>> GetAllApprovals(int agentId)
        {
            try
            {
                var data = await _sfpMappingApprovalService.GetMappingApprovalsbasedonAgent(agentId);
                return ResponseModel<SfpMappingApproval>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMappingApproval>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getapprovalsbasedonagentandstatus")]
        public async Task<ResponseModel<SfpMappingApproval>> GetAllApprovalsbasedonAgentandStatus(int agentId,string status)
        {
            try
            {
                var data = await _sfpMappingApprovalService.GetMappingApprovalsbasedonAgentandStatus(agentId, status);
                return ResponseModel<SfpMappingApproval>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMappingApproval>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getapprovalsbasedoncustomerandstatus")]
        public async Task<ResponseModel<SfpMappingApproval>> GetAllApprovalsbasedonCustomerandStatus(int customerId, string status)
        {
            try
            {
                var data = await _sfpMappingApprovalService.GetMappingApprovalsbasedonCustomerandStatus(customerId, status);
                return ResponseModel<SfpMappingApproval>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMappingApproval>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("saveapproval")]
        public async Task<ResponseModel<SfpMappingApproval>> SaveApproval([FromBody] SfpMappingApproval sfpMappingApproval)
        {
            try
            {
                await _sfpMappingApprovalService.SaveMappingApproval(sfpMappingApproval);
                return ResponseModel<SfpMappingApproval>.ToApiResponse("Success", "Mapping Approval Save Successful", new List<SfpMappingApproval>() { new SfpMappingApproval { Id= sfpMappingApproval .Id} });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMappingApproval>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("updateapproval")]
        public async Task<ResponseModel<SfpMappingApproval>> UpdateApproval([FromBody] SfpMappingApproval sfpMappingApproval)
        {
            try
            {
                await _sfpMappingApprovalService.UpdateMappingApproval(sfpMappingApproval);
                return ResponseModel<SfpMappingApproval>.ToApiResponse("Success", "Mapping Approval Save Successful", new List<SfpMappingApproval>() { new SfpMappingApproval { Id = sfpMappingApproval.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMappingApproval>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        //[HttpPost]
        //[Route("updateapproval")]
        //public async Task<ResponseModel<SfpMappingApproval>> UpdateApproval([FromBody] SfpMappingApproval sfpMappingApproval)
        //{
        //    try
        //    {
        //        await _sfpMappingApprovalService.UpdateMappingApproval(sfpMappingApproval);
        //        return ResponseModel<SfpMappingApproval>.ToApiResponse("Success", "Mapping Approval Save Successful", null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ResponseModel<SfpMappingApproval>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
        //    }
        //}
    }
}
