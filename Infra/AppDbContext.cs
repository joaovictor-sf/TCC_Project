using Microsoft.EntityFrameworkCore;
using TCC_WPF.Model;

namespace TCC_WPF.Infra
{
    class AppDbContext : DbContext {

        public DbSet<ProcessLog> ProcessLogs { get; set; }
        public DbSet<InactivityLog> InactivityLogs { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // Exemplo de string de conexão PostgreSQL
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=TCC_Test;Username=postgres;Password=postgres");
        }
    }
}
