using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly ISfpCityMasterService _sfpCityMasterService;
        private readonly ILogger<CityController> _logger;
        public CityController(ISfpCityMasterService sfpCityMasterService, ILogger<CityController> logger)
        {
            _sfpCityMasterService = sfpCityMasterService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getcities")]
        public async Task<ResponseModel<SfpCityMaster>> GetCities(int stateid)
        {
            try
            {
                var data = await _sfpCityMasterService.GetCities(stateid);
                return ResponseModel<SfpCityMaster>.ToApiResponse("Success","List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCityMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("addcity")]
        public async Task<ResponseModel<SfpCityMaster>> SaveCity([FromBody]SfpCityMaster city)
        {
            try
            {
                await _sfpCityMasterService.SaveCity(city);
                return ResponseModel<SfpCityMaster>.ToApiResponse("Success", "City Master Save Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCityMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
