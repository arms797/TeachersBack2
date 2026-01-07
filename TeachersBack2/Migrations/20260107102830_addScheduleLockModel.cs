using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachersBack2.Migrations
{
    /// <inheritdoc />
    public partial class addScheduleLockModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lock",
                table: "WeeklySchedules");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "WeeklySchedules");

            migrationBuilder.CreateTable(
                name: "ScheduleLocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CenterCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeacherCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Term = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleLocks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleLocks");

            migrationBuilder.AddColumn<bool>(
                name: "Lock",
                table: "WeeklySchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "WeeklySchedules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
