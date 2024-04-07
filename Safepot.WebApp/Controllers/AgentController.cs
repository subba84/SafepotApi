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
        private readonly ISfpCompanyService _companyService;
        private readonly ISfpStateMasterService _stateService;
        private readonly ISfpCityMasterService _cityService;
        private readonly ISfpSubscriptionHistoryService _sfpSubscriptionHistoryService;
        private int? _loggedInUserId;
        private string? _loggedInUserName;
        public AgentController(ISfpUserService userService,
            ISfpCompanyService companyService,
            ISfpStateMasterService stateService,
            ISfpCityMasterService cityService,
            ILogger<AgentController> logger,
            ISfpActivityLogService activityLogService,
            ISfpSubscriptionHistoryService sfpSubscriptionHistoryService)
        {
            _userService = userService;
            _companyService = companyService;
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
                var agents = await _companyService.GetAllCompanies();
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
                SfpCompany company = new SfpCompany();
                if(agentid > 0)
                {
                    company = await _companyService.GetCompany(agentid);
                }
                return View(company);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while Fetch Agent Details..", ex);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAgent(SfpCompany company)
        {
            try
            {
                var companies = await _companyService.GetAllCompanies();
                if(companies!=null && companies.Count() > 0)
                {
                    var existedCompany = companies.Where(x => x.Mobile == company.Mobile);
                    if(existedCompany!=null && existedCompany.Count() > 0)
                    {
                        TempData["Notification"] = "Company Already Exists";
                        return RedirectToAction("List");
                    }
                }
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                if (company.Id > 0)
                {
                    var existingdata = await _companyService.GetCompany(company.Id);
                    if (company.CityId > 0 && company.StateId > 0)
                    {
                        IEnumerable<SfpCityMaster> cities = await _cityService.GetCities(company.StateId ?? 0);
                        if (cities != null && cities.Count() > 0)
                            company.CityName = cities.First(x => x.Id == company.CityId).CityName;
                        IEnumerable<SfpStateMaster> states = await _stateService.GetStates();
                        if (states != null && states.Count() > 0)
                            company.StateName = states.First(x => x.Id == company.StateId).StateName;
                    }
                    company.ApprovalStatus = "Submitted";
                    company.ActionPerformedBy = _loggedInUserName;
                    company.ActionPerformedOn = DateTime.Now;
                    await _companyService.UpdateCompany(company);

                    // Save Subscription History
                    if (existingdata.TotalAmount!= company.TotalAmount || existingdata.RenewalDate != existingdata.RenewalDate)
                    {
                        SfpSubscriptionHistory history = new SfpSubscriptionHistory();
                        history.AgentId = company.Id;
                        history.AgentName = company.CompanyName;
                        history.SubscriptionPrice = company.TotalAmount;
                        history.RenewalDate = company.RenewalDate;
                        history.CreatedBy = _loggedInUserId;
                        history.CreatorName = _loggedInUserName;
                        history.CreatedOn = DateTime.Now;
                        await _sfpSubscriptionHistoryService.SaveSubscriptionHistory(history);
                    }                    
                }
                else
                {                    
                    if(company.CityId > 0 && company.StateId > 0)
                    {
                        IEnumerable<SfpCityMaster> cities = await _cityService.GetCities(company.StateId ?? 0);
                        if (cities != null && cities.Count() > 0)
                            company.CityName = cities.First(x => x.Id == company.CityId).CityName;
                        IEnumerable<SfpStateMaster> states = await _stateService.GetStates();
                        if (states != null && states.Count() > 0)
                            company.StateName = states.First(x => x.Id == company.StateId).StateName;
                    }
                    company.ApprovalStatus = "Submitted";
                    company.ActionPerformedBy = _loggedInUserName;
                    company.ActionPerformedOn = DateTime.Now;
                    company.ActionRemarks = "Approved by System";
                    await _companyService.SaveCompany(company);
                    await _activityLogService.SaveActivityLog("Save Agent", _loggedInUserName + " has created an agent - " + company.CompanyName , null, _loggedInUserId, _loggedInUserName ?? "");
                    
                }
                TempData["Notification"] = "Company Saved Successfully";
                return RedirectToAction("List");
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while Agent Creation..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Delete(int companyId)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                var user = await _companyService.GetCompany(companyId);
                await _companyService.DeleteCompany(companyId);
                await _activityLogService.SaveActivityLog("Delete Company", _loggedInUserName + " has deleted an company - " + user.CompanyName, null, _loggedInUserId, _loggedInUserName ?? "");
                TempData["Notification"] = "Company Deleted";
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
