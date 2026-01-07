using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachersBack2.Migrations
{
    /// <inheritdoc />
    public partial class AddLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lock",
                table: "WeeklySchedules");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "WeeklySchedules");
        }
    }
}
