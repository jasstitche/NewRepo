using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Enum
{
    public class eFashionEnum
    {
        public enum PaymentType
        {
            Transfer = 1,
                Cash = 2,

        }

        public enum PaymentVerificationStatus
        {
            Sent = 1,
            Pending = 2,
            Completed = 3,
            Seen = 4, 
            Declined = 5,
        }

        public enum OrderStatus 
        { 
            Awaiting = 1,
            Shipped = 2,
            Completed = 3,
            Cancelled = 4,
            Recieved =  5,
        }
    }   
    
}
