namespace TeachersBack2.DTOs;

public class UserCreateDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string NationalCode { get; set; } = default!;
    public string Mobile { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string CenterCode { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public List<int>? RoleIds { get; set; }
}

public class UserEditDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? NationalCode { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string CenterCode { get; set; }
    public bool IsActive { get; set; }
    public List<int>? RoleIds { get; set; } // اختصاص یا حذف نقش‌ها
}

public class UserReadDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string NationalCode { get; set; } = default!;
    public string Mobile { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string CenterCode { get; set; }
    public string Username { get; set; } = default!;
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
}
