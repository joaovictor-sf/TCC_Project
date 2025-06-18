using Microsoft.EntityFrameworkCore;
using TCC_MVVM.Model;
using TCC_MVVM.Model.Enum;

namespace TCC_MVVM.Infra
{
    /// <summary>
    /// Representa o contexto de acesso ao banco de dados PostgreSQL para a aplicação TCC_MVVM.
    /// Gerencia os conjuntos de entidades (DbSets) e as configurações do modelo.
    /// </summary>
    class AppDbContext : DbContext {

        /// <summary>
        /// Tabela de logs de uso de processos por usuários.
        /// </summary>
        public DbSet<ProcessLog> ProcessLogs { get; set; }
        /// <summary>
        /// Tabela de logs de inatividade dos usuários.
        /// </summary>
        public DbSet<InactivityLog> InactivityLogs { get; set; }
        /// <summary>
        /// Tabela de usuários do sistema.
        /// </summary>
        public DbSet<UserModel> Users { get; set; }
        /// <summary>
        /// Tabela de logs de jornada de trabalho diária.
        /// </summary>
        public DbSet<DailyWorkLog> DailyWorkLogs { get; set; }

        /// <summary>
        /// Configura a string de conexão com o banco de dados PostgreSQL.
        /// </summary>
        /// <param name="optionsBuilder">Construtor de opções para o contexto.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // ALTERE ESTA STRING SE ESTIVER USANDO OUTRA INSTÂNCIA DO POSTGRES
            var connectionString = "Host=localhost;Port=5432;Database=TCC_MVVM;Username=postgres;Password=postgres";
            optionsBuilder.UseNpgsql(connectionString);
        }

        /// <summary>
        /// Configura o modelo de dados, relacionamentos e seed inicial.
        /// </summary>
        /// <param name="modelBuilder">Builder de modelo de dados.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
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

            base.OnModelCreating(modelBuilder);
        }
    }
}
