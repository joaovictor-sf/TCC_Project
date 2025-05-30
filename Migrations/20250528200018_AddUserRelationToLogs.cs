using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TCC_MVVM.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRelationToLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ProcessLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserModelId",
                table: "ProcessLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "InactivityLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserModelId",
                table: "InactivityLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    WorkHours = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "IsActive", "LastName", "Name", "PasswordHash", "Role", "Username", "WorkHours" },
                values: new object[] { 1, "admin@sistema.com", true, "Master", "Admin", "$2a$11$.VCJ0QEhQDU/nxKqypncf.3Vi6a2Xil3.6Vq1ewVT9e4kqjn3bM1i", "ADMIN", "admin", "OITO_HORAS" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessLogs_UserId",
                table: "ProcessLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessLogs_UserModelId",
                table: "ProcessLogs",
                column: "UserModelId");

            migrationBuilder.CreateIndex(
                name: "IX_InactivityLogs_UserId",
                table: "InactivityLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InactivityLogs_UserModelId",
                table: "InactivityLogs",
                column: "UserModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_InactivityLogs_Users_UserId",
                table: "InactivityLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InactivityLogs_Users_UserModelId",
                table: "InactivityLogs",
                column: "UserModelId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessLogs_Users_UserId",
                table: "ProcessLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessLogs_Users_UserModelId",
                table: "ProcessLogs",
                column: "UserModelId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InactivityLogs_Users_UserId",
                table: "InactivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_InactivityLogs_Users_UserModelId",
                table: "InactivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessLogs_Users_UserId",
                table: "ProcessLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessLogs_Users_UserModelId",
                table: "ProcessLogs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_ProcessLogs_UserId",
                table: "ProcessLogs");

            migrationBuilder.DropIndex(
                name: "IX_ProcessLogs_UserModelId",
                table: "ProcessLogs");

            migrationBuilder.DropIndex(
                name: "IX_InactivityLogs_UserId",
                table: "InactivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_InactivityLogs_UserModelId",
                table: "InactivityLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ProcessLogs");

            migrationBuilder.DropColumn(
                name: "UserModelId",
                table: "ProcessLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InactivityLogs");

            migrationBuilder.DropColumn(
                name: "UserModelId",
                table: "InactivityLogs");
        }
    }
}
