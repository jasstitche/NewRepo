using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class SamplePage
    {
        public int? id { get; set; }
        public string? File { get; set; }
        public string? DesignName { get; set; }
        public string? MaterialName { get; set; }
        public string? ClothSize { get; set; }
        public decimal? Price { get; set; }
        public DateTime? DateSampled { get; set; }
        public int? NumberOfItem { get; set; }
        [NotMapped]
        public IFormFile SampleImage { get; set; }
        
    }
}
