using Microsoft.EntityFrameworkCore;
using TCC_MVVM.Model;
using TCC_MVVM.Model.Enum;

namespace TCC_MVVM.Infra
{
    class AppDbContext : DbContext {

        public DbSet<ProcessLog> ProcessLogs { get; set; }
        public DbSet<InactivityLog> InactivityLogs { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<DailyWorkLog> DailyWorkLogs { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // ALTERE ESTA STRING SE ESTIVER USANDO OUTRA INSTÂNCIA DO POSTGRES
            var connectionString = "Host=localhost;Port=5432;Database=TCC_MVVM;Username=postgres;Password=postgres";
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            //modelBuilder.HasPostgresEnum<UserRole>();
            //modelBuilder.HasPostgresEnum<WorkHours>();

            modelBuilder.Entity<ProcessLog>()
                        .HasOne(p => p.User)
                        .WithMany(u => u.ProcessLogs)
                        .HasForeignKey(p => p.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InactivityLog>()
                        .HasOne(p => p.User)
                        .WithMany(u => u.InactivityLogs)
                        .HasForeignKey(p => p.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserModel>()
                        .Property(u => u.Role)
                        .HasConversion<string>();

            modelBuilder.Entity<UserModel>()
                        .Property(u => u.WorkHours)
                        .HasConversion<string>();

            modelBuilder.Entity<UserModel>().HasData(new UserModel
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "$2a$11$.VCJ0QEhQDU/nxKqypncf.3Vi6a2Xil3.6Vq1ewVT9e4kqjn3bM1i",//admin123
                Name = "Admin",
                LastName = "Master",
                Email = "admin@sistema.com",
                Role = UserRole.ADMIN,
                WorkHours = WorkHours.OITO_HORAS,
                IsActive = true
            });

            modelBuilder.Entity<DailyWorkLog>()
                       .HasOne(log => log.User)
                       .WithMany()
                       .HasForeignKey(log => log.UserId)
                       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyWorkLog>()
                        .HasIndex(log => new { log.UserId, log.Date })
                        .IsUnique(); // Um log por usuário por dia

            modelBuilder.Entity<DailyWorkLog>()
                        .Property(d => d.Date)
                        .HasColumnType("timestamp without time zone");

            base.OnModelCreating(modelBuilder);
        }
    }
}
