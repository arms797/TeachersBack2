using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachersBack2.Migrations
{
    /// <inheritdoc />
    public partial class addExamSeatModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamSeats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StdNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShSh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceCenter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestCenter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeatNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StdBuildingNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Classroom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Row = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSeats", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamSeats");
        }
    }
}
