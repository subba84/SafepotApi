using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Safepot.Business;
using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MastersController : ControllerBase
    {
        private readonly ISfpMakeMasterService _sfpMakeMasterService;
        private readonly ISfpModelMasterService _sfpModelMasterService;
        private readonly ISfpUomMasterService _sfpUomMasterService;
        private readonly ISfpUserService _sfpUserService;
        private readonly ISfpAgentCustDeliveryMapService _sfpAgentCustDeliveryMapService;
        private readonly ILogger<MastersController> _logger;
        public MastersController(ISfpMakeMasterService sfpMakeMasterService,
            ISfpModelMasterService sfpModelMasterService,
            ISfpUomMasterService sfpUomMasterService,
            ISfpUserService sfpUserService,
            ISfpAgentCustDeliveryMapService sfpAgentCustDeliveryMapService,
            ILogger<MastersController> logger)
        {
            _sfpMakeMasterService = sfpMakeMasterService;
            _sfpModelMasterService = sfpModelMasterService;
            _sfpUomMasterService = sfpUomMasterService;
            _sfpUserService = sfpUserService;
            _sfpAgentCustDeliveryMapService = sfpAgentCustDeliveryMapService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getmakes")]
        public async Task<ResponseModel<SfpMakeMaster>> GetMakes()
        {
            try
            {
                var data = await _sfpMakeMasterService.GetMakes();
                return ResponseModel<SfpMakeMaster>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMakeMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("addmake")]
        public async Task<ResponseModel<SfpMakeMaster>> SaveMake([FromBody] SfpMakeMaster make)
        {
            try
            {
                await _sfpMakeMasterService.SaveMake(make);
                return ResponseModel<SfpMakeMaster>.ToApiResponse("Success", "Make Master Save Successful", new List<SfpMakeMaster>() { new SfpMakeMaster { Id= make.Id} });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpMakeMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getmodels")]
        public async Task<ResponseModel<SfpModelMaster>> GetModels()
        {
            try
            {
                var data = await _sfpModelMasterService.GetModels();
                return ResponseModel<SfpModelMaster>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpModelMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("addmodel")]
        public async Task<ResponseModel<SfpModelMaster>> SaveModel([FromBody] SfpModelMaster model)
        {
            try
            {
                await _sfpModelMasterService.SaveModel(model);
                return ResponseModel<SfpModelMaster>.ToApiResponse("Success", "Model Master Save Successful", new List<SfpModelMaster>() { new SfpModelMaster { Id = model.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpModelMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getuoms")]
        public async Task<ResponseModel<SfpUomMaster>> GetUoms()
        {
            try
            {
                var data = await _sfpUomMasterService.GetUoms();
                return ResponseModel<SfpUomMaster>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUomMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("saveuom")]
        public async Task<ResponseModel<SfpUomMaster>> SaveUom([FromBody] SfpUomMaster uom)
        {
            try


            {
                await _sfpUomMasterService.SaveUom(uom);
                return ResponseModel<SfpUomMaster>.ToApiResponse("Success", "Uom Master Save Successful", new List<SfpUomMaster>() { new SfpUomMaster { Id = uom.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUomMaster>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("cleardataandaddusers")]
        public async Task<ResponseModel<SfpUser>> ClearDataandAddUsers()
        {
            try
            {
                await _sfpUomMasterService.ClearData();
                // Vendor Creation
                for (int i = 1; i < 10; i++)
                {
                    SfpUser sfpUser = new SfpUser();
                    sfpUser.RoleId = AppRoles.Agent;
                    sfpUser.RoleName = "Agent";
                    sfpUser.Mobile = "111111111" + i.ToString();
                    sfpUser.EmailId = "Vendor" + i.ToString() + "@gmail.com";
                    sfpUser.FirstName = "Vendor " + i.ToString();
                    sfpUser.LastName = "Test " + i.ToString();
                    sfpUser.ApprovalStatus = "Approved";
                    sfpUser.IsMobileAppInstalled = false;
                    sfpUser.Address = "Vendor " + i.ToString() + " Test";
                    await _sfpUserService.CreateUser(sfpUser);
                }

                // Customer Creation
                for (int i = 1; i < 10; i++)
                {
                    SfpUser sfpUser = new SfpUser();
                    sfpUser.RoleId = AppRoles.Customer;
                    sfpUser.RoleName = "Customer";
                    sfpUser.Mobile = "222222222" + i.ToString();
                    sfpUser.EmailId = "Customer" + i.ToString() + "@gmail.com";
                    sfpUser.FirstName = "Customer " + i.ToString();
                    sfpUser.LastName = "Test " + i.ToString();
                    sfpUser.ApprovalStatus = "Approved";
                    sfpUser.IsMobileAppInstalled = false;
                    sfpUser.Address = "Customer " + i.ToString() + " Test";
                    sfpUser.JoinDate = DateTime.Now;
                    sfpUser.StartDate = DateTime.Now;
                    await _sfpUserService.CreateUser(sfpUser);
                }

                // Delivery Boy Creation
                for (int i = 1; i < 10; i++)
                {
                    SfpUser sfpUser = new SfpUser();
                    sfpUser.RoleId = AppRoles.Delivery;
                    sfpUser.RoleName = "Delivery Boy";
                    sfpUser.Mobile = "333333333" + i.ToString();
                    sfpUser.EmailId = "Delivery Boy " + i.ToString() + "@gmail.com";
                    sfpUser.FirstName = "Delivery Boy " + i.ToString();
                    sfpUser.LastName = "Test " + i.ToString();
                    sfpUser.ApprovalStatus = "Approved";
                    sfpUser.IsMobileAppInstalled = true;
                    sfpUser.Address = "Delivery Boy " + i.ToString() + " Test";
                    sfpUser.JoinDate = DateTime.Now;
                    sfpUser.StartDate = DateTime.Now;
                    sfpUser.CreatedBy = i;
                    sfpUser.CreatorName = "Vendor " + i.ToString();
                    await _sfpUserService.CreateUser(sfpUser);

                    // Mapping with customer
                    SfpAgentCustDeliveryMap mapping = new SfpAgentCustDeliveryMap();
                    mapping.AgentId = i;
                    mapping.AgentName = "Vendor " + i.ToString();
                    mapping.DeliveryId = sfpUser.Id;
                    mapping.DeliveryName = "Delivery Boy " + i.ToString();
                    mapping.CustomerId = 9 + i;
                    mapping.CustomerName = "Customer " + i.ToString();
                    await _sfpAgentCustDeliveryMapService.SaveMapping(mapping);
                }


                return ResponseModel<SfpUser>.ToApiResponse("Success", "Data Clear Successful", new List<SfpUser>() { new SfpUser { Id = 0 } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
