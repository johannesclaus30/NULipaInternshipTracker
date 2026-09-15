namespace NULipaInternshipTracker.Models;

public class JobApplication
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public string Stage { get; set; } = "Applied";
    public string WorkSetup { get; set; } = "Remote";
    public string ExpectedSalary { get; set; } = "Unpaid";
    public string ApplicationDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public string Notes { get; set; } = string.Empty;
}