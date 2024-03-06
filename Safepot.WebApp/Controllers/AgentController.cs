using Microsoft.AspNetCore.Mvc;
using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;

namespace Safepot.WebApp.Controllers
{
    public class AgentController : Controller
    {
        private readonly ILogger<AgentController> _logger;
        private readonly ISfpActivityLogService _activityLogService;
        private readonly ISfpUserService _userService;
        private readonly ISfpStateMasterService _stateService;
        private readonly ISfpCityMasterService _cityService;
        private readonly ISfpSubscriptionHistoryService _sfpSubscriptionHistoryService;
        private int? _loggedInUserId;
        private string? _loggedInUserName;
        public AgentController(ISfpUserService userService,
            ISfpStateMasterService stateService,
            ISfpCityMasterService cityService,
            ILogger<AgentController> logger,
            ISfpActivityLogService activityLogService,
            ISfpSubscriptionHistoryService sfpSubscriptionHistoryService)
        {
            _userService = userService;
            _stateService = stateService;
            _cityService = cityService;
            _logger = logger;
            _activityLogService = activityLogService;
            _sfpSubscriptionHistoryService = sfpSubscriptionHistoryService;
        }
        public async Task<IActionResult> List()
        {
            try
            {
                var agents = await _userService.GetRolebasedUsers(AppRoles.Agent);
                //var agents = await _userService.GetAllUsers();
                return View(agents);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while Fetch Agent List..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Edit(int agentid)
        {
            try
            {
                SfpUser user = new SfpUser();
                if(agentid > 0)
                {
                    user = await _userService.GetUser(agentid);
                }
                return View(user);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while Fetch Agent Details..", ex);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAgent(SfpUser user)
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
                    user.RoleId = AppRoles.Agent;
                    user.RoleName = "Agent";
                    user.ApprovalStatus = "Approved";
                    user.ActionRemarks = "Approved by System";
                    user.ActionPerformedBy = _loggedInUserId ?? 0;
                    user.ActionPerformedOn = DateTime.Now;
                    await _userService.UpdateUser(user);

                    // Save Subscription History
                    if (existingdate.SubscriptionPrice!=user.SubscriptionPrice || existingdate.RenewalDate != user.RenewalDate)
                    {
                        SfpSubscriptionHistory history = new SfpSubscriptionHistory();
                        history.AgentId = user.Id;
                        history.AgentName = user.FirstName + " " + user.LastName;
                        history.SubscriptionPrice = user.SubscriptionPrice;
                        history.RenewalDate = user.RenewalDate;
                        history.CreatedBy = _loggedInUserId;
                        history.CreatorName = _loggedInUserName;
                        history.CreatedOn = DateTime.Now;
                        await _sfpSubscriptionHistoryService.SaveSubscriptionHistory(history);
                    }
                    
                }
                else
                {                    
                    if(user.CityId > 0 && user.StateId > 0)
                    {
                        IEnumerable<SfpCityMaster> cities = await _cityService.GetCities(user.StateId ?? 0);
                        if (cities != null && cities.Count() > 0)
                            user.CityName = cities.First(x => x.Id == user.CityId).CityName;
                        IEnumerable<SfpStateMaster> states = await _stateService.GetStates();
                        if (states != null && states.Count() > 0)
                            user.StateName = states.First(x => x.Id == user.StateId).StateName;
                    }
                    user.RoleId = AppRoles.Agent;
                    user.RoleName = "Agent";
                    user.ApprovalStatus = "Approved";
                    user.ActionPerformedBy = _loggedInUserId ?? 0;
                    user.ActionPerformedOn = DateTime.Now;
                    user.ActionRemarks = "Approved by System";
                    user.CreatedBy = _loggedInUserId ?? 0;
                    user.CreatorName = _loggedInUserName;
                    user.CreatedOn = DateTime.Now;
                    await _userService.CreateUser(user);
                    await _activityLogService.SaveActivityLog("Save Agent", _loggedInUserName + " has created an agent - " + user.FirstName + " " + user.LastName , null, _loggedInUserId, _loggedInUserName ?? "");
                    
                }
                TempData["Notification"] = "Agent Saved Successfully";
                return RedirectToAction("List");
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while Agent Creation..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Delete(int agentid)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                var user = await _userService.GetUser(agentid);
                await _userService.DeleteUser(agentid);
                await _activityLogService.SaveActivityLog("Delete Agent", _loggedInUserName + " has deleted an agent - " + user.FirstName + " " + user.LastName, null, _loggedInUserId, _loggedInUserName ?? "");
                TempData["Notification"] = "Agent Deleted";
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while Agent Deletion..", ex);
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
            catch(Exception ex)
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

        public async Task<IActionResult> PasswordReset(int agentid)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                SfpUser user = new SfpUser();
                if (agentid > 0)
                {
                    user = await _userService.GetUser(agentid);
                    user.Password = CreateString(8);
                    await _userService.UpdateUser(user);
                    await _activityLogService.SaveActivityLog("Password Reset", _loggedInUserName + " has reset password for an agent - " + user.FirstName + " " + user.LastName, null, _loggedInUserId, _loggedInUserName ?? "");
                    // Need to send an email or message to user about new password
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while Password Reset for Agent..", ex);
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
            catch(Exception ex)
            {
                _logger.LogError("Error while Creating Random String for Agent Password Reset..", ex);
                throw;
            }
        }
    }
}
