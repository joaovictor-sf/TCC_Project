using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_MVVM.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProcessLogDateField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "ProcessLogs",
                newName: "Date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "DailyWorkLogs",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "ProcessLogs",
                newName: "StartTime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "DailyWorkLogs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
