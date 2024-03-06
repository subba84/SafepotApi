using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpPriceMaster
    {
        public int Id { get; set; }
        public int? MakeModelMasterId { get; set; }
        public decimal? Price { get; set; }
        public bool? IsActive { get; set; }
    }
}
