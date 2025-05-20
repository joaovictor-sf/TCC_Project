using Microsoft.EntityFrameworkCore;
using TCC_MVVM.Model;

namespace TCC_MVVM.Infra
{
    class AppDbContext : DbContext {

        public DbSet<ProcessLog> ProcessLogs { get; set; }
        public DbSet<InactivityLog> InactivityLogs { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // ALTERE ESTA STRING SE ESTIVER USANDO OUTRA INSTÂNCIA DO POSTGRES
            var connectionString = "Host=localhost;Port=5432;Database=TCC_MVVM;Username=postgres;Password=postgres";
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
