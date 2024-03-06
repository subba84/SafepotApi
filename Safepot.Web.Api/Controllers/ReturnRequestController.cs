using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using NPOI.POIFS.Crypt.Dsig;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReturnRequestController : ControllerBase
    {
        private readonly ISfpReturnQuantityService _sfpReturnQuantityService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private readonly ISfpUserService _sfpUserService;
        private readonly ILogger<ReturnRequestController> _logger;
        IWebHostEnvironment _hostingEnv;
        private readonly IMapper _mapper;
        public ReturnRequestController(ISfpReturnQuantityService sfpReturnQuantityService, ILogger<ReturnRequestController> logger,
            IWebHostEnvironment environment, IMapper mapper,
            ISfpMakeModelMasterService sfpMakeModelMasterService,
            ISfpUserService sfpUserService
            )
        {
            _sfpReturnQuantityService = sfpReturnQuantityService;
            _logger = logger;
            _hostingEnv = environment;
            _mapper = mapper;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
            _sfpUserService = sfpUserService;
        }

        [HttpGet]
        [Route("getreturnrequestdatabasedonid")]
        public async Task<ResponseModel<ReturnQuantityModel>> GetReturnRequestbasedonid(int id)
        {
            try
            {
                var data = await _sfpReturnQuantityService.GetReturnRequestData(id);
                ReturnQuantityModel returnQuantityModel = new ReturnQuantityModel();// _mapper.Map<ReturnQuantityModel>(data);
                returnQuantityModel.Id = data.Id;
                returnQuantityModel.TransactionId = data.TransactionId;
                returnQuantityModel.CustomerId = data.CustomerId;
                returnQuantityModel.FileUrls = new List<byte[]>();
                var folderPath = Path.Combine(_hostingEnv.ContentRootPath, "Uploads", returnQuantityModel.TransactionId ?? "");
                if (Directory.Exists(folderPath))
                {
                    string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                    if(files.Length > 0)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            //string[] fileparts = files[i].Split("Uploads");
                            //returnQuantityModel.FileUrls.Add("\\Uploads" + fileparts[1]);
                            returnQuantityModel.FileUrls.Add(System.IO.File.ReadAllBytes(files[i]));
                        }
                    }
                    //returnQuantityModel.FileUrls = files.ToList();
                }
                return ResponseModel<ReturnQuantityModel>.ToApiResponse("Success", "List Available", new List<ReturnQuantityModel> { returnQuantityModel });
            }
            catch (Exception ex)
            {
                return ResponseModel<ReturnQuantityModel>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getreturnrequestdataforcustomer")]
        public async Task<ResponseModel<SfpReturnQuantity>> GetReturnRequests(int customerId,int agentId)
        {
            try
            {
                var data = await _sfpReturnQuantityService.GetReturnRequestsDatabasedonCustomer(customerId,agentId);
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getorderedproductsbasedoncustomerandagent")]
        public async Task<ResponseModel<SfpCustomizeQuantity>> GetOrderedProductsbasedonCustomerandAgent(int customerId, int agentId)
        {
            try
            {
                var data = await _sfpReturnQuantityService.GetProductsbasedonAgentandCustomerforReturn(agentId, customerId,DateTime.Now.Date);
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomizeQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getreturnrequestdata")]
        public async Task<ResponseModel<SfpReturnQuantity>> GetReturnRequests()
        {
            try
            {
                var data = await _sfpReturnQuantityService.GetReturnRequestsData();
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("savereturnrequestdata")]
        public async Task<ResponseModel<SfpReturnQuantity>> SaveReturnRequestData([FromBody]ReturnQuantityModel returnQuantityModel /*SfpReturnQuantity sfpReturnQuantity*/)
        {
            try
            {
                SfpReturnQuantity sfpReturnQuantity = _mapper.Map<SfpReturnQuantity>(returnQuantityModel);
                Guid id = Guid.NewGuid();
                string transactionId = id.ToString();
                sfpReturnQuantity.TransactionId = transactionId;

                var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModel(sfpReturnQuantity.MakeModelId == null ? 0 : Convert.ToInt32(sfpReturnQuantity.MakeModelId));
                sfpReturnQuantity.Price = makeModelMasterData.Price;
                sfpReturnQuantity.MakeName = makeModelMasterData.MakeName;
                sfpReturnQuantity.ModelName = makeModelMasterData.ModelName;
                sfpReturnQuantity.UomName = makeModelMasterData.UomName;
                sfpReturnQuantity.UnitQuantity = Convert.ToString(makeModelMasterData.Quantity);

                bool isDirectApproval = false;
                var customer = await _sfpUserService.GetUser(sfpReturnQuantity.CustomerId ?? 0);
                if(customer!=null && customer.Id > 0)
                {
                    if(customer.IsMobileAppInstalled == false || customer.IsMobileAppInstalled == null)
                    {
                        isDirectApproval = true;
                    }
                }
                // Auto approve if agent is creating the damage return on behalf of Customer...
                if (isDirectApproval == true && sfpReturnQuantity.AgentId > 0 && sfpReturnQuantity.Status == "Pending" 
                    && (sfpReturnQuantity.IsForVendorApproval == null || sfpReturnQuantity.IsForVendorApproval == false))
                {
                    sfpReturnQuantity.Status = "Approved";
                    sfpReturnQuantity.ActionPerformedBy = sfpReturnQuantity.AgentId;
                    sfpReturnQuantity.ActionPerformedOn = DateTime.Now;
                }
                await _sfpReturnQuantityService.SaveReturnRequest(sfpReturnQuantity);
                if(returnQuantityModel.Files!=null && returnQuantityModel.Files.Count() > 0)
                {
                    string uploads = Path.Combine(_hostingEnv.ContentRootPath, "Uploads", transactionId);
                    if (!System.IO.Directory.Exists(uploads))
                    {
                        System.IO.Directory.CreateDirectory(uploads);
                    }
                    foreach (var file in returnQuantityModel.Files)
                    {
                        System.IO.File.WriteAllBytes(uploads + "/" + file.Key,Convert.FromBase64String(file.Value));
                    }
                }




                //IEnumerable<IFormFile> images = returnQuantityModel.files;
                //Guid id = Guid.NewGuid();
                //SfpReturnQuantity sfpReturnQuantity = _mapper.Map<SfpReturnQuantity>(returnQuantityModel);
                //sfpReturnQuantity.TransactionId = id.ToString();
                //await _sfpReturnQuantityService.SaveReturnRequest(sfpReturnQuantity);
                //if(images!=null && images.Count() > 0)
                //{
                //    string uploads = Path.Combine(_hostingEnv.ContentRootPath, "Uploads", sfpReturnQuantity.TransactionId);
                //    if (!System.IO.Directory.Exists(uploads))
                //    {
                //        System.IO.Directory.CreateDirectory(uploads);
                //    }
                //    foreach(var image in images)
                //    {
                //        using(var stream = System.IO.File.Create(uploads + "/" + image.FileName))
                //     {
                //            await image.CopyToAsync(stream);
                //        }
                //    }
                //}
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Success", "Return Request Save Successful", new List<ReturnQuantityModel> { new ReturnQuantityModel { Id= sfpReturnQuantity.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updatereturnrequestdata")]
        public async Task<ResponseModel<SfpReturnQuantity>> UpdateReturnRequestData([FromBody] SfpReturnQuantity sfpReturnQuantity)
        {
            try
            {
                var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModel(sfpReturnQuantity.MakeModelId == null ? 0 : Convert.ToInt32(sfpReturnQuantity.MakeModelId));
                sfpReturnQuantity.Price = makeModelMasterData.Price;
                sfpReturnQuantity.MakeName = makeModelMasterData.MakeName;
                sfpReturnQuantity.ModelName = makeModelMasterData.ModelName;
                sfpReturnQuantity.UomName = makeModelMasterData.UomName;
                sfpReturnQuantity.UnitQuantity = Convert.ToString(makeModelMasterData.Quantity);
                await _sfpReturnQuantityService.UpdateReturnRequest(sfpReturnQuantity);
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Success", "Return Request Update Successful", new List<SfpReturnQuantity> { new ReturnQuantityModel { Id = sfpReturnQuantity.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deletereturnrequestdata")]
        public async Task<ResponseModel<SfpReturnQuantity>> DeleteReturnRequestData(int id)
        {
            try
            {
                await _sfpReturnQuantityService.DeleteReturnRequest(id);
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Success", "Return Request Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("approvereturnrequest")]
        public async Task<ResponseModel<SfpReturnQuantity>> ApproveReturnRequest(int requestId,int approvedBy,string status)
        {
            try
            {
                SfpReturnQuantity sfpReturnQuantity = await _sfpReturnQuantityService.GetReturnRequestData(requestId);
                if(sfpReturnQuantity!=null && sfpReturnQuantity.Id > 0)
                {
                    sfpReturnQuantity.ActionPerformedBy = approvedBy;
                    sfpReturnQuantity.ActionPerformedOn = DateTime.Now;
                    sfpReturnQuantity.Status = status;
                    await _sfpReturnQuantityService.UpdateReturnRequest(sfpReturnQuantity);
                }
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Success", "Return Request " + status + " Successfully", new List<ReturnQuantityModel> { new ReturnQuantityModel { Id = sfpReturnQuantity == null ? 0 : sfpReturnQuantity.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getdamagereturnrequestsforcustomerapproval")]
        public async Task<ResponseModel<SfpReturnQuantity>> GetDamageReturnRequestsforCustomerApproval(int customerId, int agentId, string status)
        {
            try
            {
                var data = await _sfpReturnQuantityService.GetReturnRequestsforCustomerApproval(customerId, agentId,status);
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getdamagereturnrequestsforagentapproval")]
        public async Task<ResponseModel<SfpReturnQuantity>> GetDamageReturnRequestsforAgentApproval(int customerId, int agentId,string status)
        {
            try
            {
                var data = await _sfpReturnQuantityService.GetReturnRequestsforAgentApproval(customerId, agentId,status);
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpReturnQuantity>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
