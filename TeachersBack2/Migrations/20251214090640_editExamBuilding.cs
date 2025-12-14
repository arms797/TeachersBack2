using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachersBack2.Migrations
{
    /// <inheritdoc />
    public partial class editExamBuilding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StdBuildingNo",
                table: "ExamSeats",
                newName: "BuildingNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BuildingNo",
                table: "ExamSeats",
                newName: "StdBuildingNo");
        }
    }
}
