using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpMakeMaster
    {
        [Key]
        public int Id { get; set; }
        public string? MakeName { get; set; }
        public bool? IsActive { get; set; }
    }
}
