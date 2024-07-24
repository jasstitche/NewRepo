using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
    public class ShopViewModel
    {
        public IEnumerable<SamplePageViewModel> SamplePageViewModel { get; set; }
        public IEnumerable<CartViewModels> CartViewModels { get; set; }


    }
}
