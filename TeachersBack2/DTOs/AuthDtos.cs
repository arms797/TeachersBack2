namespace TeachersBack2.DTOs;

public record LoginDto(string Username, string Password);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);
public record UpdateContactDto(string? Mobile, string? Email);
