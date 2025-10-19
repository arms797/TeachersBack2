using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachersBack2.Migrations
{
    /// <inheritdoc />
    public partial class addTeachersModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldOfStudy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Center = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CooperationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcademicRank = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutivePosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsNeighborTeaching = table.Column<bool>(type: "bit", nullable: false),
                    NeighborCenters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suggestion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeeklySchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Center = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    A = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    B = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    C = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    D = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    E = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlternativeHours = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForbiddenHours = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklySchedules", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "WeeklySchedules");
        }
    }
}
