using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Core.Enum.eFashionEnum;

namespace Core.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public string PaidBy { get; set; }
        [ForeignKey("PaidBy")]
        public virtual ApplicationUser? Applicationuser { get; set; }
        public string ApprovedBy { get; set; }
        [ForeignKey("ApprovedBy")]

        public virtual ApplicationUser? ApplicationsUser { get; set; }
        public int? OrdersId { get; set; }

        [ForeignKey("OrdersId")]
        public virtual Orders? Orders { get; set; }
        public PaymentVerificationStatus PaymentVerificationStatus { get; set; }
        public decimal? TotalAmount { get; set; }
        public PaymentType PaymentType { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? ApprovedDate {  get; set; }
        public string? PaymentReceipt {  get; set; }
        [NotMapped]
        public IFormFile UploadPayment{ get; set; }
    }
}
