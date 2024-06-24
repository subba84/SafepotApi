using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpOtp
    {
        [Key]
        public int Id { get; set; }
        [SkipEncrypt]
        public string? Otp { get; set; }
        [SkipEncrypt]
        public string? Mobile { get; set; }
        public bool? IsValidated { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
