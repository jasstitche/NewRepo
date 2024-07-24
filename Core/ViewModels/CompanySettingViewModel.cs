using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
    public class CompanySettingViewModel
    {
        public string? CompanyAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public string? BankName { get; set; }
        public int? PickUpDays { get; set; }
        public string? DeliveryFee { get; set; }
    }
}
