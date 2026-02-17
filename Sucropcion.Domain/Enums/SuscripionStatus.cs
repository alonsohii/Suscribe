using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suscripcion.Domain.Enums
{
    public enum SuscripcionStatus
    {
        Pending,
        Active,
        Expired,
        Cancelled,
        PaymentFailed
    }
}
