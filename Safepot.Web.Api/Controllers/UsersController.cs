using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using Quartz.Logging;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Safepot.Web.Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly INotificationService _sfpNotificationService;
        private readonly ISfpUserService _sfpUserService;
        private readonly IUserRoleMapService _userRoleMapService;
        private readonly ISfpOtpService _sfpOtpService;
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        public UsersController(ISfpUserService sfpUserService,
            ILogger<UsersController> logger,
            IMapper mapper, 
            IUserRoleMapService userRoleMapService,
            INotificationService sfpNotificationService,
            IHttpClientFactory clientFactory,
            ISfpOtpService sfpOtpService,
            IConfiguration configuration)
        {
            _sfpUserService = sfpUserService;
            _userRoleMapService = userRoleMapService;
            _logger = logger;
            _mapper = mapper;
            _sfpNotificationService = sfpNotificationService;
            _clientFactory = clientFactory;
            _sfpOtpService = sfpOtpService;
            _configuration = configuration;
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
                string firstName=string.Empty,lastName=string.Empty,creatorName =string.Empty;
                firstName = user.FirstName ?? "";
                lastName = user.LastName ?? "";
                creatorName = user.CreatorName ?? "";
                var existinguser = await _sfpUserService.GetUserbyMobileNumberandRole(user.Mobile ?? "", user.RoleId ?? 0);
                if(existinguser!=null && existinguser.Id > 0)
                    return ResponseModel<SfpUserDto>.ToApiResponse("Failure", "User Already Existed with Same Mobile Number and Role", null);
                await _sfpUserService.CreateUser(user);
                var data = await _sfpUserService.GetUser(user.Id);
                var userRoles = await _userRoleMapService.GetAllRolesbyUser(data.Id);
                SfpUserDto sfpUserDto = new SfpUserDto();
                sfpUserDto = _mapper.Map<SfpUserDto>(data);
                sfpUserDto.UserRoles = userRoles;

                string description = string.Empty;
                int agentId = 0,customerId=0,deliveryBoyId=0;
                if(user.RoleId == 4)
                {
                    customerId = user.Id;
                    if (user.CreatedBy > 0)
                    {
                        agentId = (int)user.CreatedBy;
                        description = "Customer - " + firstName + " " + lastName + " have been created by " + creatorName;
                    }
                    else
                    {
                        description = "Customer - " + firstName + " " + lastName + " have been created in the system";
                    }                    
                }
                else if (user.RoleId == 5)
                {
                    deliveryBoyId = user.Id;
                    if (user.CreatedBy > 0)
                    {
                        agentId = (int)user.CreatedBy;
                        description = "Delivery Boy - " + firstName + " " + lastName + " have been created by " + creatorName;
                    }
                    else
                    {
                        description = "Delivery Boy - " + firstName + " " + lastName + " have been created in the system";
                    }
                }
                await _sfpNotificationService.CreateNotification(description, agentId, customerId, deliveryBoyId,null, "New User Creation", (agentId > 0 ? true : false), (customerId > 0 ? true : false), (deliveryBoyId > 0 ? true : false));
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

        [HttpGet]
        [Route("sendotp")]
        public async Task<ResponseModel<SfpUser>> SendOtp(string mobileNumber)
        {
            try
            {
                string whatsAppUserName = _configuration["WhatsAppCreds:UserName"] ?? "";
                string whatsAppPassword = _configuration["WhatsAppCreds:Password"] ?? "";
                string whatsAppDomainName = _configuration["WhatsAppCreds:DomainName"] ?? "";
                string otp = await _sfpOtpService.SaveOtp(mobileNumber);
                string requestApi = whatsAppDomainName + "api/sendmsg.php?user=" + whatsAppUserName + "&pass=" + whatsAppPassword + "&sender=BUZWAP&phone=" + mobileNumber + "&text=data_safepot_code&priority=wa&stype=normal&Params=" + otp;
                var request = new HttpRequestMessage(HttpMethod.Get, requestApi);
                request.Headers.Add("Accept", "application/vnd.github.v3+json");
                request.Headers.Add("User-Agent", "Safepot");
                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                return ResponseModel<SfpUser>.ToApiResponse("Success", "Otp sent successfully", new List<SfpUser>());
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUser>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("validateotp")]
        public async Task<ResponseModel<SfpUserDto>> ValidateOtp(string mobileNumber,string otp,bool isCustomer)
        {
            try
            {
                SfpUserDto sfpUserDto = new SfpUserDto();
                bool isOtpValidated = await _sfpOtpService.ValidateOtp(mobileNumber, otp);
                if (isOtpValidated)
                {
                    var data = await _sfpUserService.GetUserbyMobileNumber(mobileNumber, isCustomer);
                    var userRoles = await _userRoleMapService.GetRolesofUser(mobileNumber);                    
                    sfpUserDto = _mapper.Map<SfpUserDto>(data);
                    sfpUserDto.UserRoles = userRoles;
                    var userRole = (userRoles == null || userRoles.Count() == 0) ? new SfpUserRoleMap() : userRoles.First();
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, data.Mobile ?? ""),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    foreach (var role in (userRoles ?? new List<SfpUserRoleMap>()))
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, role.RoleName ?? ""));
                    }

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? ""));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddHours(3),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    sfpUserDto.Token = new JwtSecurityTokenHandler().WriteToken(token);
                    sfpUserDto.ExpirationToken = token.ValidTo;
                    return ResponseModel<SfpUserDto>.ToApiResponse("Success", "Otp Validation Successful", new List<SfpUserDto>() { sfpUserDto });
                }
                else
                {
                    return ResponseModel<SfpUserDto>.ToApiResponse("Fail", "Otp Validation Failed", new List<SfpUserDto>() { sfpUserDto });
                }                
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpUserDto>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
        //public async Task SendWhatsAppMessage(string mobileNumber,string otp)
        //{
        //    try
        //    {
        //        string whatsAppUserName = "Data Bricks Technologies01";
        //        string whatsAppPassword = "123456";
        //        string requestApi = "http://bhashsms.com/" + "api/sendmsg.php?user=" + whatsAppUserName + "&pass=" + whatsAppPassword + "&sender=BUZWAP&phone=" + mobileNumber + "&text=sfp_otp_tmp&priority=wa&stype=normal&Params=" + otp;
        //        var request = new HttpRequestMessage(HttpMethod.Get,requestApi);
        //        request.Headers.Add("Accept", "application/vnd.github.v3+json");
        //        request.Headers.Add("User-Agent", "Safepot");
        //        var client = _clientFactory.CreateClient();
        //        var response = await client.SendAsync(request);
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
    }
}
