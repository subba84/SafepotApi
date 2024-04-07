using Microsoft.AspNetCore.Mvc;
using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;

namespace Safepot.WebApp.Controllers
{
    public class AgentApprovalController : Controller
    {
        private readonly ILogger<AgentApprovalController> _logger;
        private readonly ISfpActivityLogService _activityLogService;
        private readonly ISfpUserService _userService;
        private readonly IUserRoleMapService _userRoleMapService;
        private readonly ISfpCompanyService _companyService;
        private readonly ISfpInvoiceService _invoiceService;
        private int? _loggedInUserId;
        private string? _loggedInUserName;
        public AgentApprovalController(ISfpUserService userService,
            ISfpCompanyService companyService,
            IUserRoleMapService userRoleMapService,
            ISfpInvoiceService invoiceService,
            ILogger<AgentApprovalController> logger,
            ISfpActivityLogService activityLogService)
        {
            _userService = userService;
            _logger = logger;
            _companyService = companyService;
            _userRoleMapService = userRoleMapService;
            _invoiceService = invoiceService;
            _activityLogService = activityLogService;
        }
        public async Task<IActionResult> List()
        {
            try
            {
                var companies = await _companyService.GetAllCompanies();
                if(companies != null && companies.Count() > 0)
                {
                    companies = companies.Where(x=>x.ApprovalStatus == "Submitted");
                    if(companies!=null && companies.Count() > 0)
                        return View(companies.ToList());
                }
                return View(new List<SfpCompany>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Fetch Company List for Approval..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Edit(int companyId)
        {
            try
            {
                SfpCompany company = new SfpCompany();
                if (companyId > 0)
                {
                    company = await _companyService.GetCompany(companyId);
                }
                return View(company);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Fetch Company Details for Approval..", ex);
                throw;
            }
        }

        public async Task<IActionResult> ApproveAgent(int companyId,string status,string remarks)
        {
            try
            {
                SfpCompany company = new SfpCompany();
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                if (companyId > 0)
                {
                    company = await _companyService.GetCompany(companyId);
                    // Agent Creation
                    SfpUser user = new SfpUser();
                    user.CompanyName = company.CompanyName;
                    user.Mobile = company.Mobile;
                    user.EmailId = company.EmailId;
                    user.PANNumber = company.PANNumber;
                    user.StateId = company.StateId;
                    user.StateName = company.StateName;
                    user.CityId = company.CityId;
                    user.CityName = company.CityName;
                    user.StartDate = company.FromDate;
                    user.ApprovalStatus = "Approved";
                    user.Address = company.Address;
                    user.LandMark = company.LandMark;
                    user.SubscriptionPrice = company.TotalAmount;
                    user.RoleId = AppRoles.Agent;
                    user.RoleName = "Agent";

                    // Invoice Creation
                    SfpInvoice sfpInvoice = new SfpInvoice();
                    sfpInvoice.CompanyId = companyId;
                    sfpInvoice.CompanyName = company.CompanyName;
                    sfpInvoice.CompanyAddress = company.Address;
                    sfpInvoice.CompanyState = company.StateName;
                    sfpInvoice.CompanyCity = company.CityName;
                    sfpInvoice.GSTNumber = "36AAIFD1660R1ZZ";
                    sfpInvoice.Amount = company.Amount;
                    sfpInvoice.GST = company.GST;
                    sfpInvoice.TotalAmount = company.TotalAmount;
                    sfpInvoice.CustomerPAN = company.PANNumber;
                    sfpInvoice.CreatedOn = DateTime.Now;
                    

                    company.ApprovalStatus = status;
                    company.ActionRemarks = remarks;
                    company.ActionPerformedBy = _loggedInUserName;
                    company.ActionPerformedOn = DateTime.Now;
                    if (status == "Approved")
                    {
                        company.FromDate = DateTime.Now.Date;
                        company.ToDate = DateTime.Now.Date.AddYears(1);
                        user.StartDate = company.FromDate;

                        sfpInvoice.FromDate = company.FromDate;
                        sfpInvoice.ToDate = company.ToDate;
                    }
                        await _companyService.UpdateCompany(company);
                    if(status == "Approved")
                    {
                        await _userService.CreateUser(user);

                        SfpUserRoleMap rolemap = new SfpUserRoleMap();
                        rolemap.UserId = user.Id;
                        rolemap.RoleId = AppRoles.Agent;
                        rolemap.RoleName = "Agent";
                        rolemap.CreatedOn = DateTime.Now;
                        await _userRoleMapService.SaveUserRole(rolemap);

                        await _invoiceService.SaveInvoice(sfpInvoice);

                    }
                }
                await _activityLogService.SaveActivityLog("Company Approval", _loggedInUserName + " has " + status + " a company - " + company.CompanyName, null, _loggedInUserId, _loggedInUserName ?? "");
                TempData["Notification"] = "Company Registration " + status + " Successfully";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Company Approval..", ex);
                throw;
            }
        }
    }
}
