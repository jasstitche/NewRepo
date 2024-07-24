using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
    public class CartViewModels
    {

        public int? Id { get; set; }
        public int? SampleId { get; set; }
        public virtual SamplePage? SamplesPage { get; set; }
        public int? Quantity { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? UserId { get; set; }
        public string? ReturnUrl { get; set; }
        public virtual ApplicationUser? AppUser { set; get; }

    }
}
