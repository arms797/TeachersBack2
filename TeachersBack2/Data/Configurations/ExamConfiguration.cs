using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachersBack2.Models;

namespace TeachersBack2.Data.Configurations
{
    public class ExamConfiguration : IEntityTypeConfiguration<Exam>
    {
        public void Configure(EntityTypeBuilder<Exam> builder)
        {
            // ایندکس برای فیلدهایی که جستجو می‌شوند
            builder.HasIndex(e => e.Teacher).HasDatabaseName("IX_Exams_Teacher");
            builder.HasIndex(e => e.TeacherCode).HasDatabaseName("IX_Exams_TeacherCode");
            builder.HasIndex(e => e.CenterCode).HasDatabaseName("IX_Exams_CenterCode");
            builder.HasIndex(e => e.Center).HasDatabaseName("IX_Exams_Center");
            builder.HasIndex(e => e.LessonNoGrp).HasDatabaseName("IX_Exams_LessonNoGrp");
            builder.HasIndex(e => e.ExamDate).HasDatabaseName("IX_Exams_ExamDate");
            builder.HasIndex(e => e.ExamType).HasDatabaseName("IX_Exams_ExamType");
            builder.HasIndex(e => e.QuestionDesigner).HasDatabaseName("IX_Exams_QuestionDesigner");
            builder.HasIndex(e => e.SourceNo).HasDatabaseName("IX_Exams_SourceNo");
            builder.HasIndex(e => e.DayOfWeek).HasDatabaseName("IX_Exams_DayOfWeek");
        }
    }
}