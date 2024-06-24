using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private readonly ISfpStateMasterService _sfpStateMasterService;
        private readonly ILogger<StateController> _logger;
        public StateController(ISfpStateMasterService sfpStateMasterService, ILogger<StateController> logger)
        {
            _sfpStateMasterService = sfpStateMasterService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getstates")]
        public async Task<ResponseModel<SfpStateMaster>> GetStates()
        {
            try
            {
                var data = await _sfpStateMasterService.GetStates();
                return ResponseModel<SfpStateMaster>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpStateMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("addstate")]
        public async Task<ResponseModel<SfpStateMaster>> SaveState([FromBody] SfpStateMaster state)
        {
            try
            {
                await _sfpStateMasterService.SaveState(state);
                return ResponseModel<SfpStateMaster>.ToApiResponse("Success", "State Addition Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpStateMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
