using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpUomMaster
    {
        [Key]
        public int Id { get; set; }
        public string? UomName { get; set; }
        public bool? IsActive { get; set; }
    }
}
