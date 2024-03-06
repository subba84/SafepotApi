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
    public class StockEntryController : ControllerBase
    {
        private readonly ISfpStockInwardEntryService _sfpStockInwardEntryService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ILogger<StockEntryController> _logger;
        public StockEntryController(ISfpStockInwardEntryService sfpStockInwardEntryService,
            ILogger<StockEntryController> logger,
            ISfpMakeModelMasterService sfpMakeModelMasterService)
        {
            _sfpStockInwardEntryService = sfpStockInwardEntryService;
            _logger = logger;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
        }

        [HttpGet]
        [Route("getstockentries")]
        public async Task<ResponseModel<SfpInwardStockEntry>> GetStockEntries(int agentid)
        {
            try
            {
                var data = await _sfpStockInwardEntryService.GetStockEntries(agentid);
                return ResponseModel<SfpInwardStockEntry>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpInwardStockEntry>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }



        [HttpPost]
        [Route("savestockentry")]
        public async Task<ResponseModel<SfpInwardStockEntry>> SaveStockEntry([FromBody] SfpInwardStockEntry sfpInwardStockEntry)
        {
            try
            {
                var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModel(sfpInwardStockEntry.MakeModelId == null ? 0 : Convert.ToInt32(sfpInwardStockEntry.MakeModelId));
                sfpInwardStockEntry.MakeName = makeModelMasterData.MakeName;
                sfpInwardStockEntry.ModelName = makeModelMasterData.ModelName;
                sfpInwardStockEntry.UomName = makeModelMasterData.UomName;
                sfpInwardStockEntry.Price = makeModelMasterData.Price;
                await _sfpStockInwardEntryService.SaveStockEntry(sfpInwardStockEntry);
                return ResponseModel<SfpInwardStockEntry>.ToApiResponse("Success", "Stock Entry Save Successful", new List<SfpInwardStockEntry>() { new SfpInwardStockEntry { Id = sfpInwardStockEntry.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpInwardStockEntry>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatestockentry")]
        public async Task<ResponseModel<SfpInwardStockEntry>> UpdateStockEntry([FromBody] SfpInwardStockEntry sfpInwardStockEntry)
        {
            try
            {
                var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModel(sfpInwardStockEntry.MakeModelId == null ? 0 : Convert.ToInt32(sfpInwardStockEntry.MakeModelId));
                sfpInwardStockEntry.MakeName = makeModelMasterData.MakeName;
                sfpInwardStockEntry.ModelName = makeModelMasterData.ModelName;
                sfpInwardStockEntry.UomName = makeModelMasterData.UomName;
                sfpInwardStockEntry.Price = makeModelMasterData.Price;
                await _sfpStockInwardEntryService.UpdateStockEntry(sfpInwardStockEntry);
                return ResponseModel<SfpInwardStockEntry>.ToApiResponse("Success", "Stock Entry Update Successful", new List<SfpInwardStockEntry>() { new SfpInwardStockEntry { Id = sfpInwardStockEntry.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpInwardStockEntry>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deletestockentry")]
        public async Task<ResponseModel<SfpInwardStockEntry>> DeleteStockEntry(int id)
        {
            try
            {
                var data = await _sfpStockInwardEntryService.GetStockEntry(id);
                await _sfpStockInwardEntryService.DeleteStockEntry(data);
                return ResponseModel<SfpInwardStockEntry>.ToApiResponse("Success", "Stock Entry Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpInwardStockEntry>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
