using Microsoft.EntityFrameworkCore;
using Suscripcion.Domain.Entities;

namespace Suscripcion.Infrastructure.Persistence
{
    /// <summary>
    /// Contexto de base de datos para la aplicación de suscripciones.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Conjunto de entidades de suscripciones.
        /// </summary>
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        
        /// <summary>
        /// Conjunto de entidades de usuarios.
        /// </summary>
        public DbSet<User> Users => Set<User>();

        /// <summary>
        /// Inicializa el contexto con las opciones de configuración.
        /// </summary>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// Configura el modelo aplicando todas las configuraciones de entidades.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
