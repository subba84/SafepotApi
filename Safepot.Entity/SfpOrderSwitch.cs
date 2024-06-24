using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpOrderSwitch
    {
        [Key]
        public int Id { get; set; }
        public int? AgentId { get; set; }
        public int? CustomerId { get; set; }
        public bool? IsOrderGenerationOff { get; set; }
        public DateTime? OrderGenerateOnOffFrom { get; set; }
    }
}
