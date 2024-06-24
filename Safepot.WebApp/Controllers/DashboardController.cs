using Microsoft.AspNetCore.Mvc;
using Safepot.Contracts;
using Safepot.WebApp.Models;

namespace Safepot.WebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ISfpSubscriptionHistoryService _sfpSubscriptionHistoryService;
        public DashboardController(ISfpSubscriptionHistoryService sfpSubscriptionHistoryService)
        {
            _sfpSubscriptionHistoryService = sfpSubscriptionHistoryService;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                DashboardModel dashboardModel = new DashboardModel();
                dashboardModel.AgentsExpiredinThreeMonths = await _sfpSubscriptionHistoryService.GetAgentsexpiredinThreeMonths();
                dashboardModel.AgentsSubscriptions = await _sfpSubscriptionHistoryService.GetAgentSubscriptions();
                return View(dashboardModel);
            }
            catch(Exception ex)
            {
                throw ex;
            }            
        }
    }
}
