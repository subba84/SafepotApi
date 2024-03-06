using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpStateMasterService : ISfpStateMasterService
    {
        private readonly ISfpDataRepository<SfpStateMaster> _stateRepository;

        public SfpStateMasterService(ISfpDataRepository<SfpStateMaster> stateRepository)
        {
            _stateRepository = stateRepository;
        }

        public async Task<IEnumerable<SfpStateMaster>> GetStates()
        {
            try
            {
                return await _stateRepository.GetAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveState(SfpStateMaster state)
        {
            try
            {
                await _stateRepository.CreateAsync(state);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
