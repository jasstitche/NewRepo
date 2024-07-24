using Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Enum.eFashionEnum;

namespace Core.ViewModels
{
    public class PaymentViewModels
    {
        public int Id { get; set; }
        public string PaidBy { get; set; }
        [ForeignKey("PaidBy")]
        public virtual ApplicationUser? Applicationuser { get; set; }
        public string? ApprovedBy { get; set; }
        [ForeignKey("ApprovedBy")]
        public virtual ApplicationUser? ApplicationsUser { get; set; }
        public PaymentVerificationStatus PaymentVerificationStatus { get; set; }
        public PaymentType PaymentType { get; set; }
        public int? OrdersId { get; set; }

        [ForeignKey("OrdersId")]
        public virtual Orders? Orders { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string PaymentReceipt { get; set; }
        [NotMapped]
        public IFormFile UploadPayment { get; set; }
    }
}
