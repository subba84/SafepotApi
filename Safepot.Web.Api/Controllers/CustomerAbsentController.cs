using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using Safepot.Contracts;
using Safepot.Entity;
using Safepot.Web.Api.Helpers;

namespace Safepot.Web.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerAbsentController : ControllerBase
    {
        private readonly ISfpCustomerAbsentService _sfpCustomerAbsentService;
        private readonly ILogger<CustomerAbsentController> _logger;
        public CustomerAbsentController(ISfpCustomerAbsentService sfpCustomerAbsentService, ILogger<CustomerAbsentController> logger)
        {
            _sfpCustomerAbsentService = sfpCustomerAbsentService;
            _logger = logger;
        }

        [HttpGet]
        [Route("getcustomerabsents")]
        public async Task<ResponseModel<SfpCustomerAbsent>> GetCustomerAbsents()
        {
            try
            {
                var data = await _sfpCustomerAbsentService.GetCustomerAbsentsData();
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getabsentdataforcustomer/{customerid}")]
        public async Task<ResponseModel<SfpCustomerAbsent>> GetAbsentDataforCustomer(int customerid, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var data = await _sfpCustomerAbsentService.GetAbsentDatabasedonCustomer(customerid, fromDate, toDate);
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpGet]
        [Route("getabsentdataforagent/{agentid}")]
        public async Task<ResponseModel<SfpCustomerAbsent>> GetAbsentDataforAgent(int agentid, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var data = await _sfpCustomerAbsentService.GetAbsentDatabasedonAgent(agentid, fromDate, toDate);
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Success", "List Available", data);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpPost]
        [Route("savecustomerabsentdata")]
        public async Task<ResponseModel<SfpCustomerAbsent>> SaveCustomerAbsentData([FromBody] SfpCustomerAbsent sfpCustomerAbsent)
        {
            try
            {
                sfpCustomerAbsent.AbsentFrom = (sfpCustomerAbsent.AbsentFrom == null ? null : sfpCustomerAbsent.AbsentFrom.Value.Date);
                sfpCustomerAbsent.AbsentTo = (sfpCustomerAbsent.AbsentTo == null ? null : sfpCustomerAbsent.AbsentTo.Value.Date);
                await _sfpCustomerAbsentService.SaveCustomerAbsentData(sfpCustomerAbsent);
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Success", "Customer Absent Data Save Successful", new List<SfpCustomerAbsent>() { new SfpCustomerAbsent { Id= sfpCustomerAbsent.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
        

        [HttpPut]
        [Route("updatecustomerabsentdata")]
        public async Task<ResponseModel<SfpCustomerAbsent>> UpdateCustomerAbsentData([FromBody] SfpCustomerAbsent sfpCustomerAbsent)
        {
            try
            {
                await _sfpCustomerAbsentService.UpdateCustomerAbsentData(sfpCustomerAbsent);
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Success", "Customer Absent Data Update Successful", new List<SfpCustomerAbsent>() { new SfpCustomerAbsent { Id = sfpCustomerAbsent.Id } });
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }

        [HttpDelete]
        [Route("deletecustomerabsentdata")]
        public async Task<ResponseModel<SfpCustomerAbsent>> DeleteCustomerAbsentData(int id)
        {
            try
            {
                await _sfpCustomerAbsentService.DeleteCustomerAbsentData(id);
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Success", "Customer Absent Data Deletion Successful", null);
            }
            catch (Exception ex)
            {
                return ResponseModel<SfpCustomerAbsent>.ToApiResponse("Failure", "Error Occured - " + ex.Message, null);
            }
        }
    }
}
