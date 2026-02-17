using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suscripcion.Application.Interfaces
{
    public interface IWebhookNotifier
    {
        Task NotifyAsync(string message, Guid? idempotencyKey = null);
    }

}
