using Microsoft.EntityFrameworkCore;
using TeachersBack2.Models;

namespace TeachersBack2.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Center> Centers => Set<Center>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Keys & Indexes
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

        // Seed data
        var adminRole = new Role { Id = 1, Title = "admin", Description = "ادمین سایت" };
        var center1 = new Center { CenterCode = 1, Title = "استان فارس" };
        var center2 = new Center { CenterCode = 6293, Title = "شیراز" };

        var adminUser = new User
        {
            Id = 1,
            FirstName = "ادمین",
            LastName = "ادمین",
            NationalCode = "0000000000",
            Mobile = "09120000000",
            Email = "admin@example.com",
            CenterCode = 1,
            Username = "admin",
            PasswordHash = "$2a$12$BhFI37anzgtbV2200UY1DO6VR2WKEOvyuZngKhhMknmIxmND12b5C",// BCrypt.Net.BCrypt.HashPassword("Admin123"),
            IsActive = true
        };

        modelBuilder.Entity<Role>().HasData(adminRole);
        modelBuilder.Entity<Center>().HasData(center1, center2);
        modelBuilder.Entity<User>().HasData(adminUser);
        modelBuilder.Entity<UserRole>().HasData(new { UserId = 1, RoleId = 1 });
    }
}
