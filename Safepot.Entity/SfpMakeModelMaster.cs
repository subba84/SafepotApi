﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpMakeModelMaster
    {
        [Key]
        public int Id { get; set; }
        public int? MakeId { get; set; }
        public string? MakeName { get; set; }
        public int? ModelId { get; set; }
        public string? ModelName { get; set; }
        public decimal? Quantity { get; set; }
        public int? UomId { get; set; }
        public string? UomName { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool? IsActive { get; set; }
        public string? Price { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; } 
    }
}
