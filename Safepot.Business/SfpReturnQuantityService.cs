using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpReturnQuantityService : ISfpReturnQuantityService
    {
        private readonly ISfpDataRepository<SfpReturnQuantity> _returnQtyRepository;
        private readonly ISfpOrderService _sfpOrderService;
        private readonly ISfpMakeModelMasterService _sfpMakeModelMasterService;

        public SfpReturnQuantityService(ISfpDataRepository<SfpReturnQuantity> returnQtyRepository, ISfpOrderService sfpOrderService, ISfpMakeModelMasterService sfpMakeModelMasterService)
        {
            _returnQtyRepository = returnQtyRepository;
            _sfpOrderService = sfpOrderService;
            _sfpMakeModelMasterService = sfpMakeModelMasterService;
        }
        public async Task DeleteReturnRequest(int id)
        {
            try
            {
                var masterdata = await _returnQtyRepository.GetAsync(x => x.Id == id);
                if (masterdata != null && masterdata.Count() > 0)
                    await _returnQtyRepository.DeleteAsync(masterdata.First());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpReturnQuantity> GetReturnRequestData(int id)
        {
            try
            {
                var masterdata = await _returnQtyRepository.GetAsync(x => x.Id == id);
                if (masterdata != null && masterdata.Count() > 0)
                    return masterdata.First();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new SfpReturnQuantity();
        }

        public async Task<IEnumerable<SfpReturnQuantity>> GetReturnRequestsData()
        {
            try
            {
                var masterdata = await _returnQtyRepository.GetAsync();
                if (masterdata != null && masterdata.Count() > 0)
                    return masterdata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpReturnQuantity>();
        }

        public async Task<IEnumerable<SfpReturnQuantity>> GetReturnRequestsDatabasedonCustomer(int customerId,int agentId)
        {
            try
            {
                var masterdata = await _returnQtyRepository.GetAsync(x=>x.CustomerId == customerId && x.AgentId == agentId);
                if (masterdata != null && masterdata.Count() > 0)
                {
                    masterdata = masterdata.Where(x => x.Status == "Approved");
                    if (masterdata != null && masterdata.Count() > 0)
                    {
                        masterdata = masterdata.Where(x => x.RefundType == "Amount");
                        if (masterdata != null && masterdata.Count() > 0)
                            return masterdata;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpReturnQuantity>();
        }

        public async Task SaveReturnRequest(SfpReturnQuantity returnRequest)
        {
            try
            {
                string status = returnRequest.Status ?? "";
                int returnQty = Convert.ToInt32(returnRequest.Quantity);
                string refundType = returnRequest.RefundType ?? "";
                await _returnQtyRepository.CreateAsync(returnRequest);
                if (status == "Approved" && refundType == "Amount")
                {
                    await _sfpOrderService.UpdateOrderQuantityonDamageReturn(returnRequest.CustomerId, returnRequest.AgentId, returnRequest.MakeModelId, DateTime.Now.Date, returnQty);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateReturnRequest(SfpReturnQuantity sfpReturnQuantity)
        {
            try
            {
                string status = sfpReturnQuantity.Status ?? "";
                int returnQty = Convert.ToInt32(sfpReturnQuantity.Quantity);
                string refundType = sfpReturnQuantity.RefundType ?? "";
                await _returnQtyRepository.UpdateAsync(sfpReturnQuantity);
                if (status == "Approved" && refundType == "Amount")
                {
                    await _sfpOrderService.UpdateOrderQuantityonDamageReturn(sfpReturnQuantity.CustomerId, sfpReturnQuantity.AgentId, sfpReturnQuantity.MakeModelId, DateTime.Now.Date, returnQty);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCustomizeQuantity>> GetProductsbasedonAgentandCustomerforReturn(int agentId, int customerId, DateTime date)
        {
            try
            {
                var orderDetails = await _sfpOrderService.GetIndividualOrder(customerId, agentId, date,"Completed");
                if(orderDetails!=null && orderDetails.Count() > 0)
                {
                    var makeModelMasterData = await _sfpMakeModelMasterService.GetMakeModels();
                    var consolidatedData = from c in orderDetails
                                           group c by new
                                           {
                                               c.MakeModelMasterId,
                                               c.Quantity
                                           } into gcs
                                           select new SfpCustomizeQuantity()
                                           {
                                               MakeModelMasterId = gcs.Key.MakeModelMasterId,
                                               Quantity = Convert.ToString(gcs.Sum(x => Convert.ToInt32(x.Quantity)))
                                           };
                    if (consolidatedData != null && consolidatedData.Count() > 0 && orderDetails.First().Status == "Completed")
                    {
                        var productMasterData = consolidatedData.ToList();
                        productMasterData.ForEach(x => {
                            var makeModelData = makeModelMasterData.First(y => y.Id == x.MakeModelMasterId);
                            x.MakeName = makeModelData.MakeName;
                            x.ModelName = makeModelData.ModelName;
                            x.UomName = makeModelData.UomName;
                            x.UnitQuantity = Convert.ToString(makeModelData.Quantity);
                            x.UnitPrice = makeModelData.Price;
                        });
                        return productMasterData;
                    }
                }
                return new List<SfpCustomizeQuantity>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpReturnQuantity>> GetReturnRequestsforCustomerApproval(int customerId, int agentId,string status)
        {
            try
            {
                var masterdata = await _returnQtyRepository.GetAsync(x => x.CustomerId == customerId && x.AgentId == agentId/* && x.IsForVendorApproval == false*/);
                if (masterdata != null && masterdata.Count() > 0)
                {
                    masterdata = masterdata.Where(x => x.Status == status);
                    if(masterdata!=null && masterdata.Count() > 0)
                        return masterdata;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpReturnQuantity>();
        }

        public async Task<IEnumerable<SfpReturnQuantity>> GetReturnRequestsforAgentApproval(int customerId, int agentId,string status)
        {
            try
            {
                var masterdata = await _returnQtyRepository.GetAsync(x => x.CustomerId == customerId && x.AgentId == agentId/* &&  x.IsForVendorApproval == true*/);
                if (masterdata != null && masterdata.Count() > 0)
                {
                    masterdata = masterdata.Where(x => x.Status == status);
                    if (masterdata != null && masterdata.Count() > 0)
                        return masterdata;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<SfpReturnQuantity>();
        }
    }
}
