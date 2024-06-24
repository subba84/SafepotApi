using iTextSharp.tool.xml.html.table;
using Microsoft.AspNetCore.Mvc;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.WebApp.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.ComponentModel.Design;
using Safepot.Business;

namespace Safepot.WebApp.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ISfpInvoiceService _sfpInvoiceService;
        private readonly ISfpUserService _sfpUserService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ISfpSubscriptionHistoryService _sfpSubscriptionHistoryService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public InvoiceController(ISfpInvoiceService sfpInvoiceService, 
            IWebHostEnvironment webHostEnvironment,
            ISfpUserService sfpUserService,
            ISfpSubscriptionHistoryService sfpSubscriptionHistoryService,
            IHttpClientFactory clientFactory,
            IConfiguration configuration)
        {
            _sfpInvoiceService = sfpInvoiceService;
            _webHostEnvironment = webHostEnvironment;
            _sfpUserService = sfpUserService;
            _sfpSubscriptionHistoryService = sfpSubscriptionHistoryService;
            _clientFactory = clientFactory;
            _configuration = configuration;
        }
        public async Task<IActionResult> InvoiceList()
        {
            try
            {
                var invoices = await _sfpInvoiceService.GetAllInvoices();
                return View(invoices);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IActionResult> SaveInvoice(SfpInvoice sfpInvoice,int AgentId)
        {
            try
            {
                string invoiceAmount = string.Empty;
                string agentName = string.Empty;
                int? _loggedInUserId = HttpContext.Session.GetInt32("_Id");
                var agent = await _sfpUserService.GetUser(AgentId);
                if(!string.IsNullOrEmpty(sfpInvoice.TotalAmount) && sfpInvoice.FromDate!=null && sfpInvoice.ToDate != null)
                {                    
                    SfpSubscriptionHistory sfpSubscriptionHistory = new SfpSubscriptionHistory();
                    sfpSubscriptionHistory.AgentId = agent.Id;
                    sfpSubscriptionHistory.AgentName = agent.FirstName + " " + agent.LastName;
                    sfpSubscriptionHistory.RenewalDate = DateTime.Now;
                    sfpSubscriptionHistory.PlanStartDate = sfpInvoice.FromDate.Value.ToString("yyyy-MM-dd");
                    sfpSubscriptionHistory.PlanEndDate = sfpInvoice.ToDate.Value.ToString("yyyy-MM-dd");
                    sfpSubscriptionHistory.SubscriptionPrice = sfpInvoice.TotalAmount;
                    sfpSubscriptionHistory.Duration = Convert.ToString((sfpInvoice.ToDate.Value - sfpInvoice.FromDate.Value).Days);
                    sfpSubscriptionHistory.CreatedBy = _loggedInUserId;
                    sfpSubscriptionHistory.CreatedOn = DateTime.Now;
                    await _sfpSubscriptionHistoryService.SaveSubscriptionHistory(sfpSubscriptionHistory);
                }
                //SfpInvoice sfpInvoice = new SfpInvoice();
                sfpInvoice.CompanyId = agent.Id;
                sfpInvoice.CompanyName = agent.CompanyName;
                sfpInvoice.CompanyAddress = agent.Address;
                sfpInvoice.CompanyState = agent.StateName;
                sfpInvoice.CompanyCity = agent.CityName;
                sfpInvoice.GSTNumber = "36AAIFD1660R1ZZ";
                //sfpInvoice.Amount = company.Amount;
                //sfpInvoice.GST = company.GST;
                //sfpInvoice.TotalAmount = company.TotalAmount;
                sfpInvoice.CustomerPAN = agent.PANNumber;
                sfpInvoice.CreatedOn = DateTime.Now;

                invoiceAmount = sfpInvoice.TotalAmount ?? "0";
                agentName = agent.FirstName + " " + agent.LastName;

                int invoiceid = await _sfpInvoiceService.SaveInvoice(sfpInvoice);

                SfpInvoice invoice = new SfpInvoice();
                invoice = await _sfpInvoiceService.GetInvoice(invoiceid);
                if (invoice != null && invoice.Id > 0)
                {
                    string path = await GenaratInvoice(invoice,false);
                    string whatsAppUserName = _configuration["WhatsAppCreds:UserName"] ?? "";
                    string whatsAppPassword = _configuration["WhatsAppCreds:Password"] ?? "";
                    string whatsAppDomainName = _configuration["WhatsAppCreds:DomainName"] ?? "";
                    string appHostName = _configuration["AppHostName"] ?? "";
                    path = Path.Combine(appHostName, path); 
                    string requestApi = whatsAppDomainName + "api/sendmsg.php?user=" + whatsAppUserName + "&pass=" + whatsAppPassword + "&sender=BUZWAP&phone=" + agent.Mobile + "&text=data_sfp_inv_template&priority=wa&stype=normal&Params=" + agentName + "," + invoiceAmount + "&htype=document&url=" + path;
                    var request = new HttpRequestMessage(HttpMethod.Get, requestApi);
                    request.Headers.Add("Accept", "application/vnd.github.v3+json");
                    request.Headers.Add("User-Agent", "Safepot");
                    var client = _clientFactory.CreateClient();
                    var response = await client.SendAsync(request);
                }
                return RedirectToAction("InvoiceList");
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IActionResult> DownloadInvoice(int invoiceId)
        {
            try
            {
                SfpInvoice invoice = new SfpInvoice();
                invoice = await _sfpInvoiceService.GetInvoice(invoiceId);
                if(invoice != null && invoice.Id > 0)
                {
                    string path = await GenaratInvoice(invoice,true);
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    return File(bytes, "application/octet-stream", "Invoice.pdf");
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }

        public async Task<string> GenaratInvoice(SfpInvoice invoice,bool isForDownload)
        {
            string result = string.Empty;
            string filePath = "";
            //string contentRootPath = _webHostEnvironment.ContentRootPath;
            string contentRootPath = _webHostEnvironment.WebRootPath;
            try
            {
                FileConversion fileConversion = new FileConversion();
                string webRootPath = _webHostEnvironment.WebRootPath;
                string viewname = "_GenarateInvoice";
                string partialview = await this.RenderViewAsync(viewname, invoice,true);
                filePath = fileConversion.SaveReport("PDF", partialview, "Invoice", contentRootPath, isForDownload);
                return filePath;
            }
            catch (Exception ex)
            {
                result = "Failure";                
            }

            return filePath;
        }

        
    }
}
