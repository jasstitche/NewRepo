using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Config
{
   public class GeneralConfiguration : IGeneralConfiguration
    {
        public string CompanyAddress { get; set; }
        public string DeliveryAddress { get; set; }

        public string AccountName { get; set; }

        public string AccountNumber { get; set; }

        public string BankName { get; set; }
        public int PickUpDays { get; set; }
        public string DeliveryFee { get; set; }

       

	}
}
