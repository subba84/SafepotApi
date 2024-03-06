using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpActivityLog
    {
        [Key]
        public int Id { get; set; }
        public string? ActivityCategory { get; set; }
        public string? ActivityDescription { get; set; }
        public int? ObjectId { get; set; }
        public int? ActivityPerformedBy { get; set; }
        public string? ActivityPerformerName { get; set; }
        public DateTime? ActivityPerformedOn { get; set; }
    }
}
