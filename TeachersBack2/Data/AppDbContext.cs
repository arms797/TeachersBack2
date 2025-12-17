using Microsoft.EntityFrameworkCore;
using TeachersBack2.Models;
using TeachersBack2.Services;

namespace TeachersBack2.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // نام کاربر جاری
    public string CurrentUser { get; set; } = "System";

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Center> Centers => Set<Center>();
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<WeeklySchedule> WeeklySchedules { get; set; }
    public DbSet<TermCalender> TermCalenders { get; set; }
    public DbSet<TeacherTerm> TeacherTerms { get; set; }
    public DbSet<ComponentFeature> ComponentFeatures { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<ExamSeat> ExamSeats { get; set; }
    public DbSet<ChangeHistory> ChangeHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // تنظیمات کلیدها و ایندکس‌ها
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.NationalCode)
            .IsUnique();

        modelBuilder.Entity<Center>()
            .HasKey(c => c.CenterCode);

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Title)
            .IsUnique();

        // Many-to-Many
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data (همان کدی که داشتی)
        var adminRole = new Role { Id = 1, Title = "admin", Description = "ادمین سایت" };
        var adminRole2 = new Role { Id = 2, Title = "centerAdmin", Description = "ادمین مرکز" };
        var adminRole3 = new Role { Id = 3, Title = "programmer", Description = "برنامه ریزی" };
        var adminRole4 = new Role { Id = 4, Title = "teacher", Description = "استاد" };
        var center1 = new Center { CenterCode = "1", Title = "ستاد استان" };
        var center2 = new Center { CenterCode = "6293", Title = "مركز شیراز" };
        var component = new ComponentFeature
        {
            Id = 1,
            Name = "ExamSeat",
            Description = "کامپوننت شماره صندلی امتحانات پایان ترم دانشجو",
            IsActive = false
        };

        var adminUser = new User
        {
            Id = 1,
            FirstName = "ادمین",
            LastName = "ادمین",
            NationalCode = "0000000000",
            Mobile = "09120000000",
            Email = "admin@example.com",
            CenterCode = "1",
            Username = "admin",
            PasswordHash = "$2a$12$BhFI37anzgtbV2200UY1DO6VR2WKEOvyuZngKhhMknmIxmND12b5C",
            IsActive = true
        };

        modelBuilder.Entity<Role>().HasData(adminRole, adminRole2, adminRole3, adminRole4);
        modelBuilder.Entity<Center>().HasData(center1, center2);
        modelBuilder.Entity<User>().HasData(adminUser);
        modelBuilder.Entity<UserRole>().HasData(new { UserId = 1, RoleId = 1 });
        modelBuilder.Entity<ComponentFeature>().HasData(component);

        string[,] arrCenters = new string[32, 2]
        {
            {"6317","مركز استهبان"},
            {"1910","مركز اوز"},
            {"1032","مركز آباده"},
            {"1049","مركز بوانات"},
            {"3974","مركز جهرم"},
            {"4106","مركز خرامه"},
            {"4533","مركز داراب"},
            {"1061","مركز صفاشهر"},
            {"3997","مركز فسا"},
            {"6811","مركز فیروزآباد"},
            {"9116","مركز كازرون"},
            {"8054","مركز لامرد"},
            {"9092","مركز نور آبادممسنی"},
            {"2825","واحد ارسنجان"},
            {"2848","واحد اقلید"},
            {"2854","واحد بیضا"},
            {"2877","واحد خاوران"},
            {"29","واحد رستم"},
            {"28","واحد زرقان"},
            {"52","واحد زرین دشت"},
            {"5764","واحد سپیدان"},
            {"6323","واحد سروستان"},
            {"2907","واحد فراشبند"},
            {"5846","واحد قیروكارزین"},
            {"5800","واحد كوار"},
            {"2913","واحد لار"},
            {"6300","واحد مرودشت"},
            {"41","واحد نودان"},
            {"4541","واحد نی ریز"},
            {"2819","واحد آباده طشك"},
            {"2883","واحد خنج"},
            {"5817","واحد مهر"},
        };

        for (int i = 0; i < 32; i++)
        {
            var c = new Center { CenterCode = arrCenters[i, 0], Title = arrCenters[i, 1] };
            modelBuilder.Entity<Center>().HasData(c);
        }
    }

    // ------------------ Audit Trail ------------------
    public override int SaveChanges()
    {
        CurrentUser = _currentUserService.GetCurrentUser();
        AddAuditTrail();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        CurrentUser = _currentUserService.GetCurrentUser();
        AddAuditTrail();
        return await base.SaveChangesAsync(cancellationToken);
    }


    private void AddAuditTrail()
    {
        var entries = ChangeTracker.Entries()
            .Where(e =>
                !(e.Entity is ChangeHistory) &&
                (e.Entity is TeacherTerm || e.Entity is WeeklySchedule) &&
                e.State == EntityState.Modified);

        var histories = new List<ChangeHistory>();

        foreach (var entry in entries)
        {
            var tableName = entry.Metadata.GetTableName();
            var recordId = (int)(entry.Property("Id").OriginalValue ?? 0);

            foreach (var prop in entry.Properties)
            {
                if (prop.IsModified)
                {
                    histories.Add(new ChangeHistory
                    {
                        TableName = tableName,
                        RecordId = recordId,
                        ColumnName = prop.Metadata.Name,
                        OldValue = prop.OriginalValue?.ToString(),
                        NewValue = prop.CurrentValue?.ToString(),
                        ChangedBy = CurrentUser,
                        ChangedAt = DateTime.UtcNow
                    });
                }
            }
        }

        if (histories.Any())
            ChangeHistory.AddRange(histories);
    }

}
