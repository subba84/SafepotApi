using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;
//using System.Web.Mvc;

namespace Safepot.WebApp.Controllers
{
    public class UploadPaymentController : Controller
    {
        private readonly ISfpUserService _userService;
        private readonly ISfpPaymentUploadService _paymentUploadService;
        private IWebHostEnvironment _environment;
        public UploadPaymentController(IWebHostEnvironment environment,ISfpUserService userService, ISfpPaymentUploadService paymentUploadService)
        {
            _userService = userService;
            _environment = environment;
            _paymentUploadService= paymentUploadService;
        }

        public async Task<IActionResult> PaymentUploads()
        {
            try
            {
                var paymentUploads = await _paymentUploadService.GetPaymentUploads();
                if(paymentUploads == null)
                {
                    return View(new List<SfpPaymentUpload>());
                }
                return View(paymentUploads.ToList());
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IActionResult> ViewPaymentUploads()
        {
            try
            {
                var paymentUploads = await _paymentUploadService.GetPaymentUploads();
                if (paymentUploads == null)
                {
                    return View(new List<SfpPaymentUpload>());
                }
                return View(paymentUploads.ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult UploadPayment()
        {
            return View();
        }

        public async Task<IActionResult> ViewPaymentUpload(int id)
        {
            try
            {
                var paymentUpload = await _paymentUploadService.GetPaymentUpload(id);
                //if (!string.IsNullOrEmpty(paymentUpload.RelativePath))
                //{
                //    string contentPath = this._environment.WebRootPath;
                //    paymentUpload.RelativePath = contentPath + paymentUpload.RelativePath;
                //}
                return View(paymentUpload);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<JsonResult> GetAgentDetails(string mobileNumber)
        {
            try
            {
                var users = await _userService.GetAllUsers();
                if(users != null && users.Count() > 0 && !string.IsNullOrEmpty(mobileNumber))
                {
                    var matchedUsers = users.Where(x => (x.Mobile ?? "").Contains(mobileNumber) && x.RoleId == AppRoles.Agent);
                    if(matchedUsers != null && matchedUsers.Count() > 0) { return Json(matchedUsers.ToList()); }
                }
                return Json("");
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IActionResult> SavePaymentDetails(int Id, IFormFile PaymentFile)
        {
            try
            {
                if(PaymentFile != null) 
                {
                    Guid guid = Guid.NewGuid();
                    string transaction = guid.ToString();
                    string wwwPath = this._environment.WebRootPath;
                    string contentPath = this._environment.ContentRootPath;
                    string path = Path.Combine(this._environment.WebRootPath, "Uploads", transaction);
                    string relativePath = "/Uploads/" + transaction + "/" + PaymentFile.FileName;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string fileName = Path.GetFileName(PaymentFile.FileName);
                    using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        PaymentFile.CopyTo(stream);
                    }

                    SfpPaymentUpload sfpPaymentUpload = new SfpPaymentUpload();
                    var agent = await _userService.GetUser(Id);
                    if(agent.Id > 0)
                    {
                        sfpPaymentUpload.AgentId = agent.Id;
                        sfpPaymentUpload.AgentName = agent.FirstName + " " + agent.LastName;
                        sfpPaymentUpload.CompanyName = agent.CompanyName;
                        sfpPaymentUpload.MobileNumber = agent.Mobile;
                        sfpPaymentUpload.RelativePath = relativePath;
                        int? _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                        sfpPaymentUpload.CreatedBy = _loggedInUserId;
                        sfpPaymentUpload.CreatedOn = DateTime.Now;
                        await _paymentUploadService.SavePaymentUpload(sfpPaymentUpload);
                    }
                }
                return RedirectToAction("PaymentUploads");
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _paymentUploadService.DeletePaymentUpload(id);
                return RedirectToAction("PaymentUploads");
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
