using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Cart

    {
        [Key]
        public int? Id { get; set; }
        public int? SampleId { get; set; }
        [ForeignKey("SampleId")]
        public virtual SamplePage? SamplesPage { get; set; }
        public int? Quantity { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? AppUser {  set; get; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }

        
    }
}
