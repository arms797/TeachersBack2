using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachersBack2.Migrations
{
    /// <inheritdoc />
    public partial class IndexExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Exams_CenterCode",
                table: "Exams",
                column: "CenterCode");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_DayOfWeek",
                table: "Exams",
                column: "DayOfWeek");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ExamDate",
                table: "Exams",
                column: "ExamDate");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ExamType",
                table: "Exams",
                column: "ExamType");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_LessonNo",
                table: "Exams",
                column: "LessonNo");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_QuestionDesigner",
                table: "Exams",
                column: "QuestionDesigner");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_SourceNo",
                table: "Exams",
                column: "SourceNo");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_Teacher",
                table: "Exams",
                column: "Teacher");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_TeacherCode",
                table: "Exams",
                column: "TeacherCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exams_CenterCode",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_DayOfWeek",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_ExamDate",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_ExamType",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_LessonNo",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_QuestionDesigner",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_SourceNo",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_Teacher",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_TeacherCode",
                table: "Exams");
        }
    }
}
