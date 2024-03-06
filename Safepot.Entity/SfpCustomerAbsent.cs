﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpCustomerAbsent
    {
        [Key]
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public DateTime? AbsentFrom { get; set; }
        public DateTime? AbsentTo { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
