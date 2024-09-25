using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Sewing
    {
        public int SewingId { get; set; } 
        public string ProductName { get; set; } 
        public string Category { get; set; } 
        public decimal BasePrice { get; set; } 
        public List<string> SizeOptions { get; set; } 
        public int TurnaroundTimeInDays { get; set; } 

    
    }
}

