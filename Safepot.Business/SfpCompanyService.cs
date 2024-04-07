using Safepot.Contracts;
using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Business
{
    public class SfpCompanyService : ISfpCompanyService
    {
        private readonly ISfpDataRepository<SfpCompany> _companyRepository;

        public SfpCompanyService(ISfpDataRepository<SfpCompany> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<int> SaveCompany(SfpCompany company)
        {
            try
            {
                return await _companyRepository.CreateAsync(company);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateCompany(SfpCompany company)
        {
            try
            {
                await _companyRepository.UpdateAsync(company);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteCompany(int companyid)
        {
            try
            {
                var companyDetails = await _companyRepository.GetAsync(x => x.Id == companyid);
                if(companyDetails!=null && companyDetails.Count() > 0)
                        await _companyRepository.DeleteAsync(companyDetails.First());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SfpCompany> GetCompany(int id)
        {
            try
            {
                var companyDetails = await _companyRepository.GetAsync(x => x.Id == id);
                if (companyDetails != null && companyDetails.Count() > 0)
                    return companyDetails.First();
                return new SfpCompany();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCompany>> GetCompaniesforApproval()
        {
            try
            {
                var companyDetails = await _companyRepository.GetAsync(x => x.ApprovalStatus == "Submitted");
                if (companyDetails != null && companyDetails.Count() > 0)
                    return companyDetails.ToList();
                return new List<SfpCompany>();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SfpCompany>> GetAllCompanies()
        {
            try
            {
                var companyDetails = await _companyRepository.GetAsync();
                if (companyDetails != null && companyDetails.Count() > 0)
                    return companyDetails.ToList();
                return new List<SfpCompany>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
