using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class CompanySetting
    {
        [Key]
        public int Id { get; set; }
        public string? CompanyAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public string? BankName { get; set; }
        public int? PickUpDays { get; set; }
        public string? DeliveryFee { get; set; }
        //public int? StateId { get; set; }
        //[ForeignKey("StateId")]

        //public virtual State? State { get; set; }

    }

}
