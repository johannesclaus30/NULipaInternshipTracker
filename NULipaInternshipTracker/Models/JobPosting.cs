using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace NULipaInternshipTracker.Models;

public class JobPosting : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Notify([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public string WorkSetup { get; set; } = "Onsite";
    public string Salary { get; set; } = "Unpaid";
    public string Description { get; set; } = string.Empty;
    public int SlotsAvailable { get; set; } = 1;
    public string PostedDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public string? CompanyLogo { get; set; }

    // Returned by SQL join if student has an existing application
    public int? ApplicationId { get; set; }

    private string? _studentApplicationStage;
    public string? StudentApplicationStage
    {
        get => _studentApplicationStage;
        set
        {
            if (_studentApplicationStage != value)
            {
                _studentApplicationStage = value;
                Notify();
                Notify(nameof(HasApplied));
                Notify(nameof(CanApply));
                Notify(nameof(IsOfferStage));
            }
        }
    }

    [JsonIgnore]
    public string PreviewDescription =>
        string.IsNullOrEmpty(Description) ? "No description provided." :
        Description.Length > 85 ? Description.Substring(0, 85) + "..." : Description;

    [JsonIgnore]
    public bool IsLongDescription => (Description?.Length ?? 0) > 85;

    [JsonIgnore]
    public bool HasApplied => !string.IsNullOrEmpty(StudentApplicationStage);

    [JsonIgnore]
    public bool CanApply => !HasApplied;

    [JsonIgnore]
    public bool IsOfferStage => !string.IsNullOrEmpty(StudentApplicationStage) &&
                                (StudentApplicationStage.Equals("Offer / MOA", StringComparison.OrdinalIgnoreCase) ||
                                 StudentApplicationStage.Equals("Offer", StringComparison.OrdinalIgnoreCase));

    [JsonIgnore]
    public bool HasLogo => !string.IsNullOrWhiteSpace(CompanyLogo);

    [JsonIgnore]
    public bool HasNoLogo => string.IsNullOrWhiteSpace(CompanyLogo);

    [JsonIgnore]
    public ImageSource? LogoSource
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CompanyLogo))
                return null;

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
}