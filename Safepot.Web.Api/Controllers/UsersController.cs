using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ISfpUserService _sfpUserService;
        private readonly IUserRoleMapService _userRoleMapService;
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;
        public UsersController(ISfpUserService sfpUserService, ILogger<UsersController> logger, IMapper mapper, IUserRoleMapService userRoleMapService)
        {
            _sfpUserService = sfpUserService;
            _userRoleMapService = userRoleMapService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("getusers")]
        public async Task<ResponseModel<SfpUser>> GetAllUsers()
        {
            try
            {                
                var data = await _sfpUserService.GetAllUsers();
                return ResponseModel<SfpUser>.ToApiResponse("Success", "Users List Available", data);
            }
            catch(Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getusersbasedonrole/{roleid}")]
        public async Task<ResponseModel<SfpUser>> GetAllUsersbasedonRole(int roleid)
        {
            try
            {
                var data = await _sfpUserService.GetAllUsersbasedonRole(roleid);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "Users List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getuserbymobilenumber/{mobilenumber}")]
        public async Task<ResponseModel<SfpUserDto>> GetUserbyMobile(string mobilenumber)
        {
            try
            {
                var data = await _sfpUserService.GetUserbyMobileNumber(mobilenumber,false);
                var userRoles = await _userRoleMapService.GetRolesofUser(mobilenumber);
                SfpUserDto sfpUserDto = new SfpUserDto();
                sfpUserDto = _mapper.Map<SfpUserDto>(data);
                sfpUserDto.UserRoles = userRoles;
                return ResponseModel<SfpUserDto>.ToApiResponse("Success", "User Available", new List<SfpUserDto>() { sfpUserDto });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUserDto>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getcustomerbymobilenumber/{mobilenumber}")]
        public async Task<ResponseModel<SfpUserDto>> GetCustomerbyMobile(string mobilenumber)
        {
            try
            {
                var data = await _sfpUserService.GetUserbyMobileNumber(mobilenumber, true);
                var userRoles = await _userRoleMapService.GetRolesofUser(mobilenumber);
                SfpUserDto sfpUserDto = new SfpUserDto();
                sfpUserDto = _mapper.Map<SfpUserDto>(data);
                sfpUserDto.UserRoles = userRoles;
                return ResponseModel<SfpUserDto>.ToApiResponse("Success", "User Available", new List<SfpUserDto>() { sfpUserDto });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUserDto>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getuserbymobilenumberandroleid")]
        public async Task<ResponseModel<SfpUser>> GetUserbyMobileandRole(string mobilenumber,int roleId)
        {
            try
            {
                var data = await _sfpUserService.GetUserbyMobileNumberandRole(mobilenumber,roleId);
                return ResponseModel<SfpUser>.ToApiResponse((data.Id > 0 ? "Success" : "Fail"), (data.Id > 0 ? "User Available" : "User Not Found"), new List<SfpUser>() { data });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("createuser")]
        public async Task<ResponseModel<SfpUserDto>> AddUser([FromBody]SfpUser user)
        {
            try
            {
                var existinguser = await _sfpUserService.GetUserbyMobileNumberandRole(user.Mobile ?? "", user.RoleId ?? 0);
                if(existinguser!=null && existinguser.Id > 0)
                    return ResponseModel<SfpUserDto>.ToApiResponse("Failure", "User Already Existed with Same Mobile Number and Role", null);
                await _sfpUserService.CreateUser(user);
                var data = await _sfpUserService.GetUser(user.Id);
                var userRoles = await _userRoleMapService.GetAllRolesbyUser(data.Id);
                SfpUserDto sfpUserDto = new SfpUserDto();
                sfpUserDto = _mapper.Map<SfpUserDto>(data);
                sfpUserDto.UserRoles = userRoles;
                //string description = (user.RoleId == 3) "Customer - " + item.CustomerName + " Order which was placed on " + (item.TransactionDate == null ? "" : item.TransactionDate.Value.ToString("dd-MM-yyyy")) + " have been rejected by system on " + DateTime.Now.ToString("dd-MM-yyyy") + " due to date change.";                
                //await _notificationService.CreateNotification(description, item.AgentId, item.CustomerId, null, (item.TransactionDate == null ? item.TransactionDate : item.TransactionDate.Value.Date), "Order Rejection", true, true, true);
                return ResponseModel<SfpUserDto>.ToApiResponse("Success", "User Available", new List<SfpUserDto>() { new SfpUserDto { Id = user.Id } });
            }
            catch(Exception ex)
            {
                return ResponseModel<SfpUserDto>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updateuser")]
        public async Task<ResponseModel<SfpUser>> UpdateUser([FromBody] SfpUser user)
        {
            try
            {
                await _sfpUserService.UpdateUser(user);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "User Update Successful", new List<SfpUserDto>() { new SfpUserDto { Id = user.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPut]
        [Route("updateappinstallstatus")]
        public async Task<ResponseModel<SfpUser>> UpdateAppInstallStatus(int customerId)
        {
            try
            {
                var user = await _sfpUserService.GetUser(customerId);
                user.IsMobileAppInstalled = true;
                await _sfpUserService.UpdateUser(user);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "User Update Successful", new List<SfpUserDto>() { new SfpUserDto { Id = user.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deleteuser")]
        public async Task<ResponseModel<SfpUser>> DeleteUser(int id)
        {
            try
            {
                await _sfpUserService.DeleteUser(id);
                var userRoles = await _userRoleMapService.GetAllRolesbyUser(id);
                if(userRoles!=null && userRoles.Count() > 0)
                {
                    foreach(var role in userRoles.ToList())
                    {
                        await _userRoleMapService.DeleteUserRole(role.Id);
                    }
                }                
                return ResponseModel<SfpUser>.ToApiResponse("Success", "User Delete Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
