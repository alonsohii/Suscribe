using Suscripcion.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suscripcion.Domain.Entities
{
    public class Subscription
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public SuscripcionStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public Subscription(int userId, SuscripcionStatus status = SuscripcionStatus.Pending)
        {
            UserId = userId;
            Status = status;
        }

        public void Activate() => Status = SuscripcionStatus.Active;
        public void Cancel() => Status = SuscripcionStatus.Cancelled;
        public void MarkAsFailed() => Status = SuscripcionStatus.PaymentFailed;
    }
}
