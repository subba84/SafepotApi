using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesReportController : ControllerBase
    {
        private readonly ISfpOrderService _sfpOrderService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ILogger<SalesReportController> _logger;
        private readonly IConfiguration _config;
        //LogWriter logWriter = null;
        public SalesReportController(ISfpOrderService sfpOrderService,
            ILogger<SalesReportController> logger,
            ISfpMakeModelMasterService sfpMakeModelMasterService,
            IConfiguration config)
        {
            _sfpOrderService = sfpOrderService;
            _logger = logger;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
            _config = config;
            //logWriter = new LogWriter(_config["LogPath"] ?? "");
        }

        [HttpGet]
        [Route("getsales")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> GetSales(int agentId, int customerid, int deliveryBoyId, int makeModelMasterId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var data = await _sfpOrderService.GetOrdersforSalesReport(customerid, agentId, deliveryBoyId, "Completed", fromDate, toDate);
                var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModels();
                if (data != null && data.Count() > 0)
                {
                    var consolidatedData = from c in data
                                           group c by new
                                           {
                                               c.CustomerId,
                                               c.MakeModelMasterId,
                                               c.Quantity,
                                               c.TotalPrice,
                                               c.TransactionDate
                                           } into gcs
                                           select new SfpCustomizeQuantity()
                                           {
                                               CustomerId = gcs.Key.CustomerId,
                                               MakeModelMasterId = gcs.Key.MakeModelMasterId,
                                               TransactionDate = gcs.Key.TransactionDate,
                                               Quantity = gcs.Sum(x => Convert.ToInt32(x.Quantity)).ToString(),
                                               TotalPrice = gcs.Sum(x => Convert.ToDouble(x.TotalPrice)).ToString()
                                           };
                    var responseData = (consolidatedData == null ? new List<SfpCustomizeQuantity>() : consolidatedData.ToList());
                    responseData.ForEach(x => {
                        var makeModelData = makeModelMasterData.First(y => y.Id == x.MakeModelMasterId);
                        x.MakeName = makeModelData.MakeName;
                        x.ModelName = makeModelData.ModelName;
                        x.UomName = makeModelData.UomName;
                        x.UnitQuantity = Convert.ToString(makeModelData.Quantity);
                        x.UnitPrice = makeModelData.Price;
                    });
                    return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", "List Available", responseData);
                }

                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", "List Available", new List<SfpCustomizeQuantity>());
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
