using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace NULipaInternshipTracker.Models;

public class JobApplication : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Notify([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public int Id { get; set; }
    public int? JobPostingId { get; set; }
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public int? CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;

    private string _stage = "Applied";
    public string Stage
    {
        get => _stage;
        set
        {
            if (_stage != value)
            {
                _stage = value;
                Notify();
                Notify(nameof(IsOfferStage));
            }
        }
    }

    public string? WorkSetup { get; set; } = "Onsite";
    public string? ExpectedSalary { get; set; } = "Unpaid";
    public string ApplicationDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public string? Notes { get; set; }
    public string? CompanyLogo { get; set; }
    public string? StudentPhoto { get; set; } // Joined from users table

    // UI Computed Properties
    [JsonIgnore]
    public bool IsOfferStage => !string.IsNullOrEmpty(Stage) &&
                                (Stage.Equals("Offer / MOA", StringComparison.OrdinalIgnoreCase) ||
                                 Stage.Equals("Offer", StringComparison.OrdinalIgnoreCase));

    [JsonIgnore]
    public bool HasLogo => !string.IsNullOrWhiteSpace(CompanyLogo);

    [JsonIgnore]
    public bool HasNoLogo => string.IsNullOrWhiteSpace(CompanyLogo);

    [JsonIgnore]
    public ImageSource? LogoSource
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CompanyLogo)) return null;
            try
            {
                byte[] bytes = Convert.FromBase64String(CompanyLogo);
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch
            {
                return null;
            }
        }
    }

    [JsonIgnore]
    public bool HasStudentPhoto => !string.IsNullOrWhiteSpace(StudentPhoto);

    [JsonIgnore]
    public bool HasNoStudentPhoto => string.IsNullOrWhiteSpace(StudentPhoto);

    [JsonIgnore]
    public ImageSource? StudentPhotoSource
    {
        get
        {
            if (string.IsNullOrWhiteSpace(StudentPhoto)) return null;
            try
            {
                byte[] bytes = Convert.FromBase64String(StudentPhoto);
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch
            {
                return null;
            }
        }
    }

    [JsonIgnore]
    public string StudentInitials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(StudentName)) return "ST";
            var parts = StudentName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }
    }
}