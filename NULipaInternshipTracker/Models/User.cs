namespace NULipaInternshipTracker.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string Role { get; set; } = "Student"; // "Student", "Company", "SuperAdmin"
    public int? CompanyId { get; set; }
}