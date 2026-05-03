using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeLang.Migrations
{
    /// <inheritdoc />
    public partial class AddLastActivityDateToStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityDate",
                table: "UserCourseStats",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActivityDate",
                table: "UserCourseStats");
        }
    }
}
