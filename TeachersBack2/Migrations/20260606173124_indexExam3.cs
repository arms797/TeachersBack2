using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachersBack2.Migrations
{
    /// <inheritdoc />
    public partial class indexExam3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exams_LessonNo",
                table: "Exams");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_LessonNoGrp",
                table: "Exams",
                column: "LessonNoGrp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exams_LessonNoGrp",
                table: "Exams");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_LessonNo",
                table: "Exams",
                column: "LessonNo");
        }
    }
}
