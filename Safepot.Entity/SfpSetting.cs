﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safepot.Entity
{
    public class SfpSetting
    {
        [Key]
        public int Id { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public bool? IsDamageReturnAllowed { get; set; }
    }
}
