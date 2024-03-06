using Microsoft.AspNetCore.Mvc;
using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;

namespace Safepot.WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly ISfpActivityLogService _activityLogService;
        private readonly ISfpUserService _userService;
        private readonly ISfpRoleMasterService _roleService;
        private int? _loggedInUserId;
        private string? _loggedInUserName;
        public UserController(ISfpUserService userService,
            ISfpRoleMasterService roleService,
            ILogger<UserController> logger,
            ISfpActivityLogService activityLogService)
        {
            _userService = userService;
            _roleService = roleService;
            _logger = logger;
            _activityLogService = activityLogService;
        }
        public async Task<IActionResult> List()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                if(users!=null && users.Count() > 0)
                {
                    users = users.Where(x => x.RoleId == AppRoles.Admin || x.RoleId == AppRoles.Accounts);
                    if (users != null && users.Count() == 0)
                        users = new List<SfpUser>();
                }
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Fetch Users List..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Edit(int userid)
        {
            try
            {
                SfpUser user = new SfpUser();
                if (userid > 0)
                {
                    user = await _userService.GetUser(userid);
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Fetch User Details..", ex);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveUser(SfpUser user)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                if (user.Id > 0)
                {
                    var existingdate = await _userService.GetUser(user.Id);
                    if (user.RoleId > 0)
                    {
                        IEnumerable<SfpRoleMaster> roles = await _roleService.GetRoles();
                        if (roles != null && roles.Count() > 0)
                            user.RoleName = roles.First(x => x.Id == user.RoleId).RoleName;
                    }
                    user.ApprovalStatus = "Approved";
                    user.ActionRemarks = "Approved by System";
                    user.ActionPerformedBy = _loggedInUserId ?? 0;
                    user.ActionPerformedOn = DateTime.Now;
                    await _userService.UpdateUser(user);
                }
                else
                {
                    if (user.RoleId > 0)
                    {
                        IEnumerable<SfpRoleMaster> roles = await _roleService.GetRoles();
                        if (roles != null && roles.Count() > 0)
                            user.RoleName = roles.First(x => x.Id == user.RoleId).RoleName;
                    }
                    user.ApprovalStatus = "Approved";
                    user.ActionRemarks = "Approved by System";
                    user.ActionPerformedBy = _loggedInUserId ?? 0;
                    user.ActionPerformedOn = DateTime.Now;
                    user.CreatedBy = _loggedInUserId ?? 0;
                    user.CreatorName = _loggedInUserName;
                    user.CreatedOn = DateTime.Now;
                    await _userService.CreateUser(user);
                    await _activityLogService.SaveActivityLog("Save User", _loggedInUserName + " has created an user - " + user.FirstName + " " + user.LastName, null, _loggedInUserId, _loggedInUserName ?? "");

                }
                TempData["Notification"] = "User Saved Successfully";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while User Creation..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Delete(int userid)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                var user = await _userService.GetUser(userid);
                await _userService.DeleteUser(userid);
                await _activityLogService.SaveActivityLog("Delete User", _loggedInUserName + " has deleted an user - " + user.FirstName + " " + user.LastName, null, _loggedInUserId, _loggedInUserName ?? "");
                TempData["Notification"] = "User Deleted";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while User Deletion..", ex);
                throw;
            }
            return RedirectToAction("List");
        }
        

        public async Task<IActionResult> PasswordReset(int userid)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                SfpUser user = new SfpUser();
                if (userid > 0)
                {
                    user = await _userService.GetUser(userid);
                    user.Password = CreateString(8);
                    await _userService.UpdateUser(user);
                    await _activityLogService.SaveActivityLog("Password Reset", _loggedInUserName + " has reset password for an customer - " + user.FirstName + " " + user.LastName, null, _loggedInUserId, _loggedInUserName ?? "");
                    // Need to send an email or message to user about new password
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Password Reset for User..", ex);
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
                _logger.LogError("Error while Creating Random String for User Password Reset..", ex);
                throw;
            }
        }
    }
}
