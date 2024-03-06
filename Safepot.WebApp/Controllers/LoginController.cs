using Microsoft.AspNetCore.Mvc;
using Safepot.Contracts;
using Safepot.DataAccess;

namespace Safepot.WebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly ILogger<LoginController> _logger;
        private readonly ISfpActivityLogService _activityLogService;
        private readonly IConfiguration _config;
        private readonly string _encryptionKey;
        const string SessionName = "_Name";
        const string SessionId = "_Id";
        const string SessionRoleId = "_RoleId";
        const string SessionRoleName = "_RoleName";
        public LoginController(ILoginService loginService, IConfiguration config, ILogger<LoginController> logger, ISfpActivityLogService activityLogService)
        {
            _loginService = loginService;
            _logger = logger;
            _config = config;
            _encryptionKey = _config["EncryptionKey"] ?? "";
            _activityLogService = activityLogService;
        }
        public IActionResult LoginPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginPost(string username,string password)
        {
            try
            {
                var user = await _loginService.CheckUser(username, password);
                if(user.Id > 0)
                {
                    HttpContext.Session.SetString(SessionName, user.FirstName + " " + user.LastName);
                    HttpContext.Session.SetInt32(SessionId, user.Id);
                    HttpContext.Session.SetString(SessionRoleName, user.RoleName ?? "");
                    HttpContext.Session.SetInt32(SessionRoleId, user.RoleId ?? 0);
                    await _activityLogService.SaveActivityLog("Login", user.FirstName + " " + user.LastName + " has been logged into Safepot", null, user.Id, user.FirstName + " " + user.LastName);
                    return RedirectToAction("Index","Dashboard");
                }
                TempData["Notification"] = "User not found";
                return RedirectToAction("LoginPage");
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while login..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Logout()
        {
            try
            {
                var _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                var _loggedInUserName = HttpContext.Session.GetString("_Name");
                HttpContext.Session.Remove(SessionId);
                HttpContext.Session.Remove(SessionName);
                HttpContext.Session.Remove(SessionRoleId);
                HttpContext.Session.Remove(SessionRoleName);
                await _activityLogService.SaveActivityLog("Log Out", _loggedInUserName + " has been logged out of Safepot", null, _loggedInUserId, _loggedInUserName ?? "");
                return RedirectToAction("LoginPage");
            }
            catch(Exception ex)
            {
                _logger.LogError("Error while logout..", ex);
                throw;
            }
        }
    }
}
