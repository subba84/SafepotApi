using Safepot.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Contracts
{
    public interface ISfpCompanyService
    {
        Task<int> SaveCompany(SfpCompany company);
        Task UpdateCompany(SfpCompany company);
        Task DeleteCompany(int companyid);
        Task<SfpCompany> GetCompany(int id);
        Task<IEnumerable<SfpCompany>> GetCompaniesforApproval();
        Task<IEnumerable<SfpCompany>> GetAllCompanies();
    }
}
