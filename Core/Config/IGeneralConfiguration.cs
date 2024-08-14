using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Config
{
    public interface IGeneralConfiguration
    {
        public string CompanyAddress { get; set; }
        public string DeliveryAddress { get; set; }

        public string AccountName { get; set; }

        public string AccountNumber { get; set; }

        public string BankName { get; set; }
        public int PickUpDays { get; set; }
        public int State { get; set; }
        public string DeliveryFee { get; set; }

    }
}
