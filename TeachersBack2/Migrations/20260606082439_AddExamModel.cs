using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachersBack2.Migrations
{
    /// <inheritdoc />
    public partial class AddExamModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CenterCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Center = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EduGrp = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TeacherCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Teacher = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LessonNoGrp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LessonNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Lesson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PracticalUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Registered = table.Column<int>(type: "int", nullable: false),
                    SourceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AttachNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TeachersCenterCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TeachersCenter = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CooperationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExamType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExamDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Start = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    End = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GroupManager = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QuestionDesigner = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Support = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QuestionDesignerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exams");
        }
    }
}
