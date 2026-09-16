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
    public string CompanyName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public int? CompanyId { get; set; }

    // Backing field for Stage so changing it updates IsOfferStage on the card
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

    public string WorkSetup { get; set; } = "Remote";
    public string ExpectedSalary { get; set; } = "Unpaid";
    public string ApplicationDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

    // Backing field for Notes so expand/collapse evaluates correctly
    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set
        {
            if (_notes != value)
            {
                _notes = value;
                Notify();
            }
        }
    }

    // Backing field for CompanyLogo to refresh logo bindings
    private string? _companyLogo;
    public string? CompanyLogo
    {
        get => _companyLogo;
        set
        {
            if (_companyLogo != value)
            {
                _companyLogo = value;
                Notify();
                Notify(nameof(HasLogo));
                Notify(nameof(HasNoLogo));
                Notify(nameof(LogoSource));
            }
        }
    }

    // Dynamic offer stage check for the "✓ Accept Offer" button
    [JsonIgnore]
    public bool IsOfferStage => !string.IsNullOrEmpty(Stage) &&
                                Stage.Equals("Offer / MOA", StringComparison.OrdinalIgnoreCase);

    // Helpers to toggle between the building emoji and the real logo
    [JsonIgnore]
    public bool HasLogo => !string.IsNullOrWhiteSpace(CompanyLogo);

    [JsonIgnore]
    public bool HasNoLogo => string.IsNullOrWhiteSpace(CompanyLogo);

    // UI state for expanding/collapsing long notes
    private bool _isNotesExpanded;
    [JsonIgnore]
    public bool IsNotesExpanded
    {
        get => _isNotesExpanded;
        set
        {
            if (_isNotesExpanded != value)
            {
                _isNotesExpanded = value;
                Notify();
                Notify(nameof(NotesMaxLines));
                Notify(nameof(ExpandCollapseButtonText));
            }
        }
    }

    [JsonIgnore]
    public int NotesMaxLines => IsNotesExpanded ? 100 : 2;

    [JsonIgnore]
    public string ExpandCollapseButtonText => IsNotesExpanded ? "Show less" : "more...";

    // Converts Base64 string directly to an ImageSource for MAUI Image controls
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