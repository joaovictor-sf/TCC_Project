using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_MVVM.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTimestampWithDateInInactivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "InactivityLogs");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "InactivityLogs",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "InactivityLogs");

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "InactivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
