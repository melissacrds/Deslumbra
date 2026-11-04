using Deslumbra.Models;
using Microsoft.EntityFrameworkCore;

namespace Deslumbra.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Representa la tabla Productos de SQL Server
        public DbSet<Producto> Productos { get; set; } = null!;
    }
}

