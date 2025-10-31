namespace TeachersBack2.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string NationalCode { get; set; } = default!;
    public string Mobile { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string CenterCode { get; set; }
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
