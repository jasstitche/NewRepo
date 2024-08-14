﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class State
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? DeliveryFee { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }


    }
}
