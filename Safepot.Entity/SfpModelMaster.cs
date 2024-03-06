using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpModelMaster
    {
        [Key]
        public int Id { get; set; }
        public string? ModelName { get; set; }
        public bool? IsActive { get; set; }
    }
}
