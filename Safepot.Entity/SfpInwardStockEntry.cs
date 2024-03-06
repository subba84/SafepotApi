using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpInwardStockEntry
    {
        [Key]
        public int Id { get; set; }
        public int? MakeModelId { get; set; }
        public string? Quantity { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? EntryDate { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? MakeName { get; set; }
        public string? ModelName { get; set; }
        public string? UomName { get; set; }
        public string? Price { get; set; }
    }
}
