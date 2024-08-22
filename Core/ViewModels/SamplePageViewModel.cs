using Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
    public class SamplePageViewModel
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

        public IFormFile? SampleImage { get; set; }
        public int ? NumberOfItems { get; set; }
        public int? SampleId { get; set; }
        public virtual SamplePage? SamplesPage { get; set; }
        public int? Quantity { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? AppUser { set; get; }
    }
}
