using Microsoft.EntityFrameworkCore;
using Suscripcion.Domain.Entities;
using Suscripcion.Domain.Interfaces;


namespace Suscripcion.Infrastructure.Persistence
{
    /// <summary>
    /// Repositorio para gestionar operaciones de usuarios y suscripciones en la base de datos.
    /// </summary>
    public class SubscriptionRepository : ISuscripcionRepository
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Inicializa el repositorio con el contexto de base de datos.
        /// </summary>
        public SubscriptionRepository(AppDbContext db) => _db = db;

        /// <summary>
        /// Agrega un nuevo usuario al contexto.
        /// </summary>
        public void AddUser(User user) => _db.Users.Add(user);

        /// <summary>
        /// Agrega una nueva suscripción al contexto.
        /// </summary>
        public void Add(Subscription s) => _db.Subscriptions.Add(s);

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        public Task<User?> GetUserByIdAsync(int userId)
            => _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

        /// <summary>
        /// Obtiene un usuario por su email.
        /// </summary>
        public Task<User?> GetUserByEmailAsync(string email)
            => _db.Users.FirstOrDefaultAsync(x => x.Email == email);

        /// <summary>
        /// Obtiene una suscripción por userId.
        /// </summary>
        public Task<Subscription?> GetByUserIdAsync(int userId)
            => _db.Subscriptions.FirstOrDefaultAsync(x => x.UserId == userId);

        /// <summary>
        /// Obtiene una suscripción con su usuario asociado 
        /// Retorna (null, null) si no existe la suscripción o el usuario.
        /// </summary>
        public async Task<(Subscription? subscription, User? user)> GetSubscriptionWithUserAsync(int userId)
        {
            var result = await _db.Subscriptions
                .Where(s => s.UserId == userId)
                .Join(
                    _db.Users,
                    subscription => subscription.UserId,
                    user => user.Id,
                    (subscription, user) => new { subscription, user }
                )
                .FirstOrDefaultAsync();

            if (result == null)
                return (null, null);

            return (result.subscription, result.user);
        }

        /// <summary>
        /// Obtiene todas las suscripciones con sus usuarios
        /// </summary>
        public Task<List<(Subscription subscription, User? user)>> GetAllWithUsersAsync() =>
            _db.Subscriptions
                .GroupJoin(_db.Users,
                    s => s.UserId,
                    u => u.Id,
                    (s, users) => new ValueTuple<Subscription, User?>(s, users.FirstOrDefault()))
                .ToListAsync();

        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos.
        /// </summary>
        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
