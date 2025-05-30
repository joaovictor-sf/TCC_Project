using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_MVVM.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InactivityLogs_Users_UserModelId",
                table: "InactivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcessLogs_Users_UserModelId",
                table: "ProcessLogs");

            migrationBuilder.DropIndex(
                name: "IX_ProcessLogs_UserModelId",
                table: "ProcessLogs");

            migrationBuilder.DropIndex(
                name: "IX_InactivityLogs_UserModelId",
                table: "InactivityLogs");

            migrationBuilder.DropColumn(
                name: "UserModelId",
                table: "ProcessLogs");

            migrationBuilder.DropColumn(
                name: "UserModelId",
                table: "InactivityLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserModelId",
                table: "ProcessLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserModelId",
                table: "InactivityLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessLogs_UserModelId",
                table: "ProcessLogs",
                column: "UserModelId");

            migrationBuilder.CreateIndex(
                name: "IX_InactivityLogs_UserModelId",
                table: "InactivityLogs",
                column: "UserModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_InactivityLogs_Users_UserModelId",
                table: "InactivityLogs",
                column: "UserModelId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessLogs_Users_UserModelId",
                table: "ProcessLogs",
                column: "UserModelId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
