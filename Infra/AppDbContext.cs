using Microsoft.EntityFrameworkCore;
using TCC_WPF.Model;

namespace TCC_WPF.Infra
{
    class AppDbContext : DbContext {

        public DbSet<UserData> UserDatas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // ALTERE ESTA STRING SE ESTIVER USANDO OUTRA INSTÂNCIA DO POSTGRES
            var connectionString = "Host=localhost;Port=5432;Database=TCC_Test;Username=postgres;Password=postgres";
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
