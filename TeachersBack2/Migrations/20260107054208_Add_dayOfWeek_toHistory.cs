using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachersBack2.Migrations
{
    /// <inheritdoc />
    public partial class Add_dayOfWeek_toHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DayOfWeek",
                table: "ChangeHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "ChangeHistory");
        }
    }
}
