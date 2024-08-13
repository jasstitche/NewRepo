using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
    public class StateViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? DeliveryFee { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }


    }
}
