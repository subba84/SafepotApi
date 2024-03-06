using Microsoft.AspNetCore.Mvc;
using Safepot.Business;
using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;

namespace Safepot.WebApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ISfpActivityLogService _activityLogService;
        private readonly ISfpUserService _userService;
        private readonly ISfpStateMasterService _stateService;
        private readonly ISfpCityMasterService _cityService;
        private int? _loggedInUserId;
        private string? _loggedInUserName;
        public CustomerController(ISfpUserService userService,
            ISfpStateMasterService stateService,
            ISfpCityMasterService cityService,
            ILogger<CustomerController> logger,
            ISfpActivityLogService activityLogService)
        {
            _userService = userService;
            _stateService = stateService;
            _cityService = cityService;
            _logger = logger;
            _activityLogService = activityLogService;
        }
        public async Task<IActionResult> List()
        {
            try
            {
                var agents = await _userService.GetRolebasedUsers(AppRoles.Customer);
                return View(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Fetch Customers List..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Edit(int customerid)
        {
            try
            {
                SfpUser user = new SfpUser();
                if (customerid > 0)
                {
                    user = await _userService.GetUser(customerid);
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Fetch Customer Details..", ex);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveCustomer(SfpUser user)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                if (user.Id > 0)
                {
                    var existingdate = await _userService.GetUser(user.Id);
                    if (user.CityId > 0 && user.StateId > 0)
                    {
                        IEnumerable<SfpCityMaster> cities = await _cityService.GetCities(user.StateId ?? 0);
                        if (cities != null && cities.Count() > 0)
                            user.CityName = cities.First(x => x.Id == user.CityId).CityName;
                        IEnumerable<SfpStateMaster> states = await _stateService.GetStates();
                        if (states != null && states.Count() > 0)
                            user.StateName = states.First(x => x.Id == user.StateId).StateName;
                    }
                    user.RoleId = AppRoles.Customer;
                    user.RoleName = "Customer";
                    user.ApprovalStatus = "Approved";
                    user.ActionRemarks = "Approved by System";
                    user.ActionPerformedBy = _loggedInUserId ?? 0;
                    user.ActionPerformedOn = DateTime.Now;
                    await _userService.UpdateUser(user);
                }
                else
                {
                    if (user.CityId > 0 && user.StateId > 0)
                    {
                        IEnumerable<SfpCityMaster> cities = await _cityService.GetCities(user.StateId ?? 0);
                        if (cities != null && cities.Count() > 0)
                            user.CityName = cities.First(x => x.Id == user.CityId).CityName;
                        IEnumerable<SfpStateMaster> states = await _stateService.GetStates();
                        if (states != null && states.Count() > 0)
                            user.StateName = states.First(x => x.Id == user.StateId).StateName;
                    }
                    user.RoleId = AppRoles.Customer;
                    user.RoleName = "Customer";
                    user.ApprovalStatus = "Approved";
                    user.ActionRemarks = "Approved by System";
                    user.ActionPerformedBy = _loggedInUserId ?? 0;
                    user.ActionPerformedOn = DateTime.Now;
                    user.CreatedBy = _loggedInUserId ?? 0;
                    user.CreatorName = _loggedInUserName;
                    user.CreatedOn = DateTime.Now;
                    await _userService.CreateUser(user);
                    await _activityLogService.SaveActivityLog("Save Customer", _loggedInUserName + " has created an customer - " + user.FirstName + " " + user.LastName, null, _loggedInUserId, _loggedInUserName ?? "");

                }
                TempData["Notification"] = "Customer Saved Successfully";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Customer Creation..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Delete(int customerid)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                var user = await _userService.GetUser(customerid);
                await _userService.DeleteUser(customerid);
                await _activityLogService.SaveActivityLog("Delete Customer", _loggedInUserName + " has deleted an customer - " + user.FirstName + " " + user.LastName, null, _loggedInUserId, _loggedInUserName ?? "");
                TempData["Notification"] = "Customer Deleted";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Customer Deletion..", ex);
                throw;
            }
            return RedirectToAction("List");
        }
        public async Task<JsonResult> GetStates()
        {
            try
            {
                var states = await _stateService.GetStates();
                return Json(states);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Getting the states list..", ex);
                throw;
            }
        }

        public async Task<JsonResult> GetCitiesbyState(int stateid)
        {
            try
            {
                var cities = await _cityService.GetCities(stateid);
                return Json(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Getting the cities list..", ex);
                throw;
            }
        }

        public async Task<IActionResult> PasswordReset(int customerid)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                SfpUser user = new SfpUser();
                if (customerid > 0)
                {
                    user = await _userService.GetUser(customerid);
                    user.Password = CreateString(8);
                    await _userService.UpdateUser(user);
                    await _activityLogService.SaveActivityLog("Password Reset", _loggedInUserName + " has reset password for an customer - " + user.FirstName + " " + user.LastName, null, _loggedInUserId, _loggedInUserName ?? "");
                    // Need to send an email or message to user about new password
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Password Reset for Customer..", ex);
                throw;
            }
            return RedirectToAction("List");
        }

        public string CreateString(int stringLength)
        {
            try
            {
                Random rd = new Random();
                const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
                char[] chars = new char[stringLength];

                for (int i = 0; i < stringLength; i++)
                {
                    chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                }

                return new string(chars);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Creating Random String for Customer Password Reset..", ex);
                throw;
            }
        }
    }
}
