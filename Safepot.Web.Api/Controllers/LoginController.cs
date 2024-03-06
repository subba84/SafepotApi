using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _LoginService;
        private readonly ILogger<LoginController> _logger;
        //ResponseModel<SfpUser> responseModel;
        public LoginController(ILoginService loginService, ILogger<LoginController> logger)
        {
            _LoginService = loginService;
            _logger = logger;
            //responseModel = new ResponseModel<SfpUser>();
        }

        [HttpGet]
        [Route("login")]
        public async Task<ResponseModel<SfpUser>> CheckUser(string username,string password)
        {
            try
            {
                var data = await _LoginService.CheckUser(username, password);
                if (data.Id > 0)
                {
                    List<SfpUser> users = new List<SfpUser>();
                    users.Add(data);
                    return ResponseModel<SfpUser>.ToApiResponse("Success", "User Found", users);
                }
                else
                {
                    return ResponseModel<SfpUser>.ToApiResponse("Failure", "User Not Found", null);
                }
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
