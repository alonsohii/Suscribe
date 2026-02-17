using Suscripcion.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suscripcion.Domain.Interfaces
{
    public interface ISuscripcionRepository
    {
        void AddUser(User user);
        void Add(Subscription subscription);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<Subscription?> GetByUserIdAsync(int userId);
        Task<(Subscription? subscription, User? user)> GetSubscriptionWithUserAsync(int userId);
        Task<List<(Subscription subscription, User? user)>> GetAllWithUsersAsync();
        Task SaveChangesAsync();
    }
}
