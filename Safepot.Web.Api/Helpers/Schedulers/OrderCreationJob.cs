using Quartz;
using Safepot.Contracts;
using Safepot.Entity;

namespace Safepot.Web.Api.Helpers.Schedulers
{
    public class OrderCreationJob : IJob
    {
        private readonly ISfpOrderService _sfpOrderService;
        public OrderCreationJob(ISfpOrderService sfpOrderService)
        {
            _sfpOrderService = sfpOrderService;
        }
        public Task Execute(IJobExecutionContext context)
        {
            
            var task = Task.Run(() => CreateOrders());
            return task;
        }
        public async Task CreateOrders()
        {
            try
            {
                await LogWriter.LogWrite("Order Creation Scheduler Started");
                await _sfpOrderService.CreateOrdersbasedonSchedule();
                await LogWriter.LogWrite("Order Creation Scheduler Completed");
            }
            catch(Exception ex)
            {
                await LogWriter.LogWrite(ex.Message + " -- " + ex.StackTrace);
            }
        }
    }
}
