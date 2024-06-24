using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public  class SfpCustomerInvoice
    {
        [Key]
        public int Id { get; set; }
        public int InvoiceYear { get; set; }
        public int InvoiceMonth { get; set; }
        public int CustomerId { get; set; }
    }
}
