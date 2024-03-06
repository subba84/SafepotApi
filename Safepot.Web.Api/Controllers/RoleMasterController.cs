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
    public class RoleMasterController : ControllerBase
    {
        private readonly ISfpRoleMasterService _sfpRoleMasterService;
        private readonly ILogger<RoleMasterController> _logger;
        public RoleMasterController(ISfpRoleMasterService sfpRoleMasterService, ILogger<RoleMasterController> logger)
        {
            _sfpRoleMasterService = sfpRoleMasterService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getroles")]
        public async Task<ResponseModel<SfpRoleMaster>> GetAllRoles()
        {
            try
            {                
                var data = await _sfpRoleMasterService.GetRoles();
                return ResponseModel<SfpRoleMaster>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpRoleMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("addrole")]
        public async Task<ResponseModel<SfpRoleMaster>> AddRole(string rolename)
        {
            try
            {
                await _sfpRoleMasterService.AddRole(rolename);
                return ResponseModel<SfpRoleMaster>.ToApiResponse("Success", "Role Addition Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpRoleMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
