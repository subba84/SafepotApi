using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;
using System.Linq;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockVarianceController : ControllerBase
    {
        private readonly ISfpStockInwardEntryService _sfpStockInwardEntryService;
        private readonly ISfpOrderService _sfpOrderService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ILogger<StockVarianceController> _logger;
        public StockVarianceController(ISfpStockInwardEntryService sfpStockInwardEntryService,
            ISfpMakeModelMasterService sfpMakeModelMasterService,
            ILogger<StockVarianceController> logger,
            ISfpOrderService sfpOrderService)
        {
            _sfpStockInwardEntryService = sfpStockInwardEntryService;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
            _logger = logger;
            _sfpOrderService = sfpOrderService;
        }

        [HttpGet]
        [Route("getstockvariancereport")]
        public async Task<ResponseModel<StockVarianceModel>> GetStockVarianceReport(int agentId)
        {
            try
            {
                var data = await _sfpStockInwardEntryService.GetStockEntries(agentId);
                var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModels();

                if (data != null && data.Count() > 0)
                {
                    var consolidatedData = from c in data
                                           group c by new
                                           {
                                               c.MakeModelId
                                           } into gcs
                                           select new StockVarianceModel()
                                           {
                                               MakeModelMasterId = (gcs.Key.MakeModelId == null ? 0 : Convert.ToInt32(gcs.Key.MakeModelId)),
                                               TotalStock = gcs.Sum(x => Convert.ToInt32(x.Quantity)),
                                           };
                    if(consolidatedData!=null && consolidatedData.Count() > 0)
                    {
                        var dataList = consolidatedData.ToList();
                        foreach (var item in dataList)
                        {
                            var masterData = makeModelMasterData.First(x => x.Id == item.MakeModelMasterId);
                            item.MakeName = masterData.MakeName;
                            item.ModelName = masterData.ModelName;
                            item.UomName = masterData.UomName;
                            item.Price = masterData.Price;
                            item.UnitQuantity = masterData.Quantity;
                            var makeModelData = await _sfpOrderService.GetOrdersbasedonMakeModel(item.MakeModelMasterId, agentId);
                            if(makeModelData!=null && makeModelData.Count() > 0)
                            {                                
                                item.SoldStock = makeModelData.Sum(x => Convert.ToInt32(x.Quantity));
                                item.Balance = item.TotalStock - makeModelData.Sum(x=>Convert.ToInt32(x.Quantity));
                            }
                        }
                        return ResponseModel<StockVarianceModel>.ToApiResponse("Success", "List Available", dataList);
                    }
                }
                return ResponseModel<StockVarianceModel>.ToApiResponse("Success", "List Available", new List<StockVarianceModel>());
            }
            catch (Exception ex)
            {
                return ResponseModel<StockVarianceModel>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
