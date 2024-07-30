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
    public class OrdersViewModel
    {
        public int? Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public int? SampleId { get; set; }
        [ForeignKey("SampleId")]
        public virtual SamplePage SamplePage { get; set; }
        public int? PaymentId { get; set; }
        [ForeignKey("PaymentId")]

        public virtual Payment Payment { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAmountToPay { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentType PaymentTypeId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryStartDate { get; set; }
        public DateTime? DeliveryEndDate { get; set; }
        public string? PickUpAddress { get; set; }
        public string? CustomersAddress { get; set; }
        public int? TotalQuantity { get; set; }
       
        public List<string?> DesignName { get; set; }
        public List<int?> Quantity { get; set; }
        public List<decimal?> SubTotal { get; set; }
        [NotMapped]
        public IFormFile UploadPayment { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        public string? City { get; set; }
        //public decimal? SubTotal { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime ApproveDate { get; set; }
        public PaymentVerificationStatus PaymentVerificationStatus { get; set; }


    }
}
