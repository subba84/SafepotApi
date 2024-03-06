using Microsoft.AspNetCore.Mvc;
using Safepot.Business.Common;
using Safepot.Contracts;
using Safepot.Entity;

namespace Safepot.WebApp.Controllers
{
    public class MakeModelController : Controller
    {
        private readonly ILogger<MakeModelController> _logger;
        private readonly ISfpActivityLogService _activityLogService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;
        private int? _loggedInUserId;
        private string? _loggedInUserName;
        public MakeModelController(ISfpMakeModelMasterService sfpMakeModelMasterService,
            ISfpActivityLogService activityLogService,
            ILogger<MakeModelController> logger)
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
        }
        public async Task<IActionResult> List()
        {
            try
            {
                var data = await _sfpMakeModelMasterService.GetMakeModels();
                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Fetch Make Model List..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                SfpMakeModelMaster makeModel = new SfpMakeModelMaster();
                if (id > 0)
                {
                    makeModel = await _sfpMakeModelMasterService.GetMakeModel(id);
                }
                return View(makeModel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Fetch Make Model Details..", ex);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveMakeModel(SfpMakeModelMaster sfpMakeModelMaster)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                if (sfpMakeModelMaster.Id > 0)
                {
                    await _sfpMakeModelMasterService.UpdateMakeModel(sfpMakeModelMaster);
                }
                else
                {
                    sfpMakeModelMaster.CreatedBy = _loggedInUserId ?? 0;
                    sfpMakeModelMaster.CreatorName = _loggedInUserName;
                    sfpMakeModelMaster.CreatedOn = DateTime.Now;
                    await _sfpMakeModelMasterService.SaveMakeModel(sfpMakeModelMaster);
                    await _activityLogService.SaveActivityLog("Save Make Model", _loggedInUserName + " has created make - " + sfpMakeModelMaster.Make + " - " + sfpMakeModelMaster.Model + " - " + sfpMakeModelMaster.Uom, null, _loggedInUserId, _loggedInUserName ?? "");
                }
                TempData["Notification"] = "Make Model Details Saved Successfully";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Make Model Creation..", ex);
                throw;
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                _loggedInUserName = HttpContext.Session.GetString("_Name");
                var data = await _sfpMakeModelMasterService.GetMakeModel(id);
                await _sfpMakeModelMasterService.DeleteMakeModel(id);
                await _activityLogService.SaveActivityLog("Delete Make Model", _loggedInUserName + " has deleted make - " + data.Make + " - " + data.Model + " - " + data.Uom, null, _loggedInUserId, _loggedInUserName ?? "");
                TempData["Notification"] = "Make Model Deleted";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Make Model Deletion..", ex);
                throw;
            }
            return RedirectToAction("List");
        }
    }
}
