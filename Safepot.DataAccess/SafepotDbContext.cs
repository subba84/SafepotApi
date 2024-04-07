using Microsoft.EntityFrameworkCore;
using Safepot.Entity;

namespace Safepot.DataAccess
{
    public class SafepotDbContext : DbContext
    {
        public SafepotDbContext(DbContextOptions<SafepotDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SfpActivityLog>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpAgentCustDeliveryMap>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpAgentCustDlivryCharge>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpCityMaster>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpCustomerAbsent>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpCustomizeQuantity>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpCutoffTimeMaster>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpInwardStockEntry>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpMakeModelMaster>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpPaymentConfirmation>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpReturnQuantity>().HasKey(k => new { k.Id });            
            modelBuilder.Entity<SfpRoleMaster>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpStateMaster>().HasKey(k => new { k.Id });            
            modelBuilder.Entity<SfpSubscriptionHistory>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpUser>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpUserRoleMap>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpPriceMaster>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpMembershipNotification>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpMakeMaster>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpModelMaster>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpUomMaster>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpMappingApproval>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpCustomerQuantity>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpPaymentReminder>().HasKey(k => new { k.Id });
            modelBuilder.Entity<Notification>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpOrder>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpSetting>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpCompany>().HasKey(k => new { k.Id });
            modelBuilder.Entity<SfpInvoice>().HasKey(k => new { k.Id });
        }
    }
}