using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceMasterController : ControllerBase
    {
        private readonly ISfpPriceMasterService _sfpPriceMasterService;
        public PriceMasterController(ISfpPriceMasterService sfpPriceMasterService)
        {
            _sfpPriceMasterService = sfpPriceMasterService;
        }

        [HttpGet]
        [Route("getpricemasterdata")]
        public async Task<ResponseModel<SfpPriceMaster>> GetPriceMasterData()
        {
            try
            {
                var data = await _sfpPriceMasterService.GetPriceMasterData();
                return ResponseModel<SfpPriceMaster>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPriceMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
        [HttpPost]
        [Route("createpricemasterdata")]
        public async Task<ResponseModel<SfpPriceMaster>> CreateMembershipp([FromBody] SfpPriceMaster sfpPriceMaster)
        {
            try
            {
                await _sfpPriceMasterService.CreatePriceMaster(sfpPriceMaster);
                return ResponseModel<SfpPriceMaster>.ToApiResponse("Success", "Price Master Saved Successfully", new List<SfpPriceMaster>() { new SfpPriceMaster { Id = sfpPriceMaster.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPriceMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatepricemasterdata")]
        public async Task<ResponseModel<SfpPriceMaster>> UpdatePriceMasterData([FromBody] SfpPriceMaster sfpPriceMaster)
        {
            try
            {
                await _sfpPriceMasterService.UpdatePriceMaster(sfpPriceMaster);
                return ResponseModel<SfpPriceMaster>.ToApiResponse("Success", "Price Master Saved Successfully", new List<SfpPriceMaster>() { new SfpPriceMaster { Id = sfpPriceMaster.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPriceMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deletepricemasterdata")]
        public async Task<ResponseModel<SfpPriceMaster>> DeletePriceMasterData(int id)
        {
            try
            {
                await _sfpPriceMasterService.DeletePriceMaster(id);
                return ResponseModel<SfpPriceMaster>.ToApiResponse("Success", "Price Master Deleted Successfully", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpPriceMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
