using Quartz;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.Entity;

namespace Safepot.Web.Api.Helpers.Schedulers
{
    public class PendingOrderRejectionJob : IJob
    {
        private readonly ISfpOrderService _sfpOrderService;

        public PendingOrderRejectionJob(ISfpOrderService sfpOrderService)
        {
            _sfpOrderService = sfpOrderService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() => RejectPendingOrders());
            return task;
        }
        public async Task RejectPendingOrders()
        {
            try
            {
                await LogWriter.LogWrite("RejectPastDatedPendingOrders Scheduler Started");
                await _sfpOrderService.RejectPastDatedPendingOrders();
                await LogWriter.LogWrite("RejectPastDatedPendingOrders Scheduler Completed");
            }
            catch (Exception ex)
            {
                await LogWriter.LogWrite(ex.Message + " -- " + ex.StackTrace);
            }
            
        }
    }
}
