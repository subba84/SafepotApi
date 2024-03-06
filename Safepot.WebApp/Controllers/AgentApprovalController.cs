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
        private int? _loggedInUserId;
        private string? _loggedInUserName;
        public AgentApprovalController(ISfpUserService userService, ILogger<AgentApprovalController> logger, ISfpActivityLogService activityLogService)
        {
            _userService = userService;
            _logger = logger;
            _activityLogService = activityLogService;
        }
        public async Task<IActionResult> List()
        {
            try
            {
                var agents = await _userService.GetRolebasedUsers(AppRoles.Agent);
                if(agents!=null && agents.Count() > 0)
                {
                    agents = agents.Where(x => x.ApprovalStatus == "Submitted");
                    if(agents != null && agents.Count() > 0)
                    {
                        agents = agents.ToList();
                    }
                }
                return View(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Fetch Agent List for Approval..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Edit(int agentid)
        {
            try
            {
                SfpUser user = new SfpUser();
                if (agentid > 0)
                {
                    user = await _userService.GetUser(agentid);
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Fetch Agent Details for Approval..", ex);
                throw;
            }
        }

        public async Task<IActionResult> ApproveAgent(int agentid,string status,string remarks)
        {
            try
            {
                SfpUser user = new SfpUser();
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                if (agentid > 0)
                {
                    user = await _userService.GetUser(agentid);
                    user.ApprovalStatus = status;
                    user.ActionRemarks = remarks;
                    user.ActionPerformedBy = _loggedInUserId ?? 0;
                    user.ActionPerformedOn = DateTime.Now;
                    await _userService.UpdateUser(user);
                }
                await _activityLogService.SaveActivityLog("Agent Approval", _loggedInUserName + " has " + status + " an agent - " + user.FirstName + " " + user.LastName, null, _loggedInUserId, _loggedInUserName ?? "");
                TempData["Notification"] = "Agent Registration " + status + " Successfully";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Agent Approval..", ex);
                throw;
            }
        }
    }
}
