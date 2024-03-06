using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleMapController : ControllerBase
    {
        private readonly IUserRoleMapService _userRoleMapService;
        private readonly ILogger<RoleMapController> _logger;
        public RoleMapController(IUserRoleMapService userRoleMapService, ILogger<RoleMapController> logger)
        {
            _userRoleMapService = userRoleMapService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getrolesbyuser/{userid}")]
        public async Task<ResponseModel<SfpUserRoleMap>> GetUserRoles(int userid)
        {
            try
            {
                var data = await _userRoleMapService.GetAllRolesbyUser(userid);
                return ResponseModel<SfpUserRoleMap>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUserRoleMap>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getrolesofuser/{mobilenumber}")]
        public async Task<ResponseModel<SfpUserRoleMap>> GetRolesofUser(string mobilenumber)
        {
            try
            {
                var data = await _userRoleMapService.GetRolesofUser(mobilenumber);
                return ResponseModel<SfpUserRoleMap>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUserRoleMap>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("saveuserrole")]
        public async Task<ResponseModel<SfpUserRoleMap>> SaveUserRole([FromBody]SfpUserRoleMap sfpUserRoleMap)
        {
            try
            {
                await _userRoleMapService.SaveUserRole(sfpUserRoleMap);
                return ResponseModel<SfpUserRoleMap>.ToApiResponse("Success", "User Role Map Save Successful", new List<SfpUserRoleMap>() { new SfpUserRoleMap { Id = sfpUserRoleMap.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUserRoleMap>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updateuserrole")]
        public async Task<ResponseModel<SfpUserRoleMap>> UpdateUserRole([FromBody] SfpUserRoleMap sfpUserRoleMap)
        {
            try
            {
                await _userRoleMapService.UpdateUserRole(sfpUserRoleMap);
                return ResponseModel<SfpUserRoleMap>.ToApiResponse("Success", "User Role Map Update Successful", new List<SfpUserRoleMap>() { new SfpUserRoleMap { Id = sfpUserRoleMap.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUserRoleMap>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deleteuserrole/{id}")]
        public async Task<ResponseModel<SfpUserRoleMap>> DeleteUserRole(int id)
        {
            try
            {
                await _userRoleMapService.DeleteUserRole(id);
                return ResponseModel<SfpUserRoleMap>.ToApiResponse("Success", "User Role Map Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUserRoleMap>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
