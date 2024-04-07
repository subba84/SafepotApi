using iTextSharp.tool.xml.html.table;
using Microsoft.AspNetCore.Mvc;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.WebApp.Helpers;

namespace Safepot.WebApp.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ISfpInvoiceService _sfpInvoiceService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InvoiceController(ISfpInvoiceService sfpInvoiceService, IWebHostEnvironment webHostEnvironment)
        {
            _sfpInvoiceService = sfpInvoiceService;
            _webHostEnvironment = webHostEnvironment;
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

        public async Task<IActionResult> DownloadInvoice(int invoiceId)
        {
            try
            {
                SfpInvoice invoice = new SfpInvoice();
                invoice = await _sfpInvoiceService.GetInvoice(invoiceId);
                if(invoice != null && invoice.Id > 0)
                {
                    string path = await GenaratInvoice(invoice);
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

        public async Task<string> GenaratInvoice(SfpInvoice invoice)
        {
            string result = string.Empty;
            string filePath = "";
            string contentRootPath = _webHostEnvironment.ContentRootPath;
            try
            {
                FileConversion fileConversion = new FileConversion();
                string webRootPath = _webHostEnvironment.WebRootPath;
                string viewname = "_GenarateInvoice";
                string partialview = await this.RenderViewAsync(viewname, invoice,true);
                filePath = fileConversion.SaveReport("PDF", partialview, "Invoice", contentRootPath);
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
