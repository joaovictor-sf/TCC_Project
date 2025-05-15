using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TCC_WPF.Migrations
{
    /// <inheritdoc />
    public partial class AddInactivityLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDatas");

            migrationBuilder.CreateTable(
                name: "InactivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalInactivity = table.Column<TimeSpan>(type: "interval", nullable: false),
                    MaxInactivity = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InactivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppName = table.Column<string>(type: "text", nullable: false),
                    WindowTitle = table.Column<string>(type: "text", nullable: false),
                    CpuTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InactivityLogs");

            migrationBuilder.DropTable(
                name: "ProcessLogs");

            migrationBuilder.CreateTable(
                name: "UserDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppName = table.Column<string>(type: "text", nullable: false),
                    CpuTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WindowTitle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDatas", x => x.Id);
                });
        }
    }
}
