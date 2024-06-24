using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class CustomerData
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public string? MobileNumber { get; set; }
        public string? Balance { get; set; }
        public string? OrderStatus { get; set; }
        public DateTime? OrderOffDate { get; set; }
    }
}
