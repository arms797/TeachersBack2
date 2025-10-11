namespace TeachersBack2.DTOs;

public class RoleCreateDto
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
}

public class RoleReadDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
}
