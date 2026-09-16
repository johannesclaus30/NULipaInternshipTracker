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
    public string Stage { get; set; } = "Applied";
    public string WorkSetup { get; set; } = "Remote";
    public string ExpectedSalary { get; set; } = "Unpaid";
    public string ApplicationDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public string Notes { get; set; } = string.Empty;
    public string? CompanyLogo { get; set; }

    // UI state for expanding/collapsing long notes (ignored by JSON/API)

    // Inside Models/JobApplication.cs

    [System.Text.Json.Serialization.JsonIgnore]
    public bool HasLogo => !string.IsNullOrWhiteSpace(CompanyLogo);

    [System.Text.Json.Serialization.JsonIgnore]
    public bool HasNoLogo => string.IsNullOrWhiteSpace(CompanyLogo);
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