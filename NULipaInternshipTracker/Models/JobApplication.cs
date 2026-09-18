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
    public string? CompanyEmail { get; set; }
    public string? StudentPhoto { get; set; }
    public string? InterviewDate { get; set; }
    public string? InterviewTime { get; set; }
    public string? InterviewLocation { get; set; }
    public string? InterviewNotes { get; set; }
    public string? RejectionReason { get; set; }

    // UI Computed Properties
    [JsonIgnore]
    public bool IsInInterviewStage => !string.IsNullOrEmpty(Stage) &&
    (Stage.Equals("Interview", StringComparison.OrdinalIgnoreCase) ||
     Stage.Equals("For Interview", StringComparison.OrdinalIgnoreCase));

    [JsonIgnore]
    public bool IsWaitlistedStage => !string.IsNullOrEmpty(Stage) &&
        (Stage.Equals("Waitlisted", StringComparison.OrdinalIgnoreCase) ||
         Stage.Equals("On Hold", StringComparison.OrdinalIgnoreCase));

    [JsonIgnore]
    public bool IsInterviewStage => !string.IsNullOrEmpty(Stage) &&
    (Stage.Equals("Interview", StringComparison.OrdinalIgnoreCase) ||
     Stage.Equals("For Interview", StringComparison.OrdinalIgnoreCase));

    [JsonIgnore]
    public bool IsRejectedStage => !string.IsNullOrEmpty(Stage) &&
        Stage.Equals("Rejected", StringComparison.OrdinalIgnoreCase);

    [JsonIgnore]
    public bool HasInterviewDetails => !string.IsNullOrWhiteSpace(InterviewDate) ||
                                       !string.IsNullOrWhiteSpace(InterviewLocation);

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
    public bool IsAccepted => !string.IsNullOrEmpty(Stage) &&
    Stage.Equals("Accepted", StringComparison.OrdinalIgnoreCase);

    [JsonIgnore]
    public bool IsSettled => !string.IsNullOrEmpty(Stage) &&
        (Stage.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ||
         Stage.Equals("Rejected", StringComparison.OrdinalIgnoreCase));

    [JsonIgnore]
    public bool CanManageStage => !IsSettled;

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