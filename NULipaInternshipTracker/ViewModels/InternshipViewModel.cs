using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.ViewModels;

public class InternshipViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Notify([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private readonly ApiService _api;
    private readonly List<JobPosting> _masterList = new();
    public ObservableCollection<JobPosting> Postings { get; } = new();

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { _isBusy = value; Notify(); } }

    private int _totalAvailable;
    public int TotalAvailable { get => _totalAvailable; set { _totalAvailable = value; Notify(); } }

    private int _appliedCount;
    public int AppliedCount { get => _appliedCount; set { _appliedCount = value; Notify(); } }

    private int _interviewCount;
    public int InterviewCount { get => _interviewCount; set { _interviewCount = value; Notify(); } }

    private int _offersAccepted;
    public int OffersAccepted { get => _offersAccepted; set { _offersAccepted = value; Notify(); } }

    private string _currentSearchQuery = string.Empty;
    private string _selectedFilter = "All";

    public ICommand RefreshCommand { get; }
    public ICommand ApplyCommand { get; }
    public ICommand AcceptOfferCommand { get; }
    public ICommand ShowDescriptionCommand { get; }

    public InternshipViewModel(ApiService api)
    {
        _api = api;
        RefreshCommand = new Command(async () => await LoadDataAsync());
        ApplyCommand = new Command<JobPosting>(async (item) => await OnApplyAsync(item));
        AcceptOfferCommand = new Command<JobPosting>(async (item) => await OnAcceptOfferAsync(item));
        ShowDescriptionCommand = new Command<JobPosting>(async (item) => await OnShowDescriptionAsync(item));
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            int studentId = UserSession.CurrentUser?.Id ?? 0;
            var list = await _api.GetJobPostingsAsync(studentId);
            _masterList.Clear();
            if (list != null) _masterList.AddRange(list);
            UpdateStats();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading postings: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateStats()
    {
        TotalAvailable = _masterList.Count(p => p.CanApply);
        AppliedCount = _masterList.Count(p => p.HasApplied);

        InterviewCount = _masterList.Count(p => !string.IsNullOrEmpty(p.StudentApplicationStage) &&
            (p.StudentApplicationStage.Equals("For Interview", StringComparison.OrdinalIgnoreCase) ||
             p.StudentApplicationStage.Equals("Interview", StringComparison.OrdinalIgnoreCase)));

        OffersAccepted = _masterList.Count(p => !string.IsNullOrEmpty(p.StudentApplicationStage) &&
            p.StudentApplicationStage.Equals("Accepted", StringComparison.OrdinalIgnoreCase));
    }

    public void Filter(string query)
    {
        _currentSearchQuery = query ?? string.Empty;
        ApplyFilters();
    }

    public void SetFilter(string filter)
    {
        _selectedFilter = string.IsNullOrWhiteSpace(filter) ? "All" : filter;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        // 👉 1. STRICTLY show only openings where the student has NOT applied yet
        var filtered = _masterList.Where(p => p.CanApply);

        // Setup filter (All, Remote, Hybrid, Onsite)
        if (!string.Equals(_selectedFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(p => p.WorkSetup != null &&
                p.WorkSetup.Equals(_selectedFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Search query
        if (!string.IsNullOrWhiteSpace(_currentSearchQuery))
        {
            filtered = filtered.Where(p =>
                (p.CompanyName != null && p.CompanyName.Contains(_currentSearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                (p.RoleTitle != null && p.RoleTitle.Contains(_currentSearchQuery, StringComparison.OrdinalIgnoreCase)));
        }

        Postings.Clear();
        foreach (var p in filtered)
        {
            Postings.Add(p);
        }
    }

    private async Task OnShowDescriptionAsync(JobPosting? item)
    {
        if (item == null) return;
        await Application.Current!.MainPage!.DisplayAlert(
            $"{item.CompanyName} - {item.RoleTitle}",
            string.IsNullOrWhiteSpace(item.Description) ? "No detailed job description provided." : item.Description,
            "Close");
    }

    private async Task OnApplyAsync(JobPosting? item)
    {
        if (item == null || item.HasApplied) return;

        string note = await Application.Current!.MainPage!.DisplayPromptAsync(
            $"Apply to {item.CompanyName}",
            $"Position: {item.RoleTitle}\n\nEnter a short note for the recruiter:",
            accept: "Submit Application",
            cancel: "Cancel",
            placeholder: "e.g., Passionate 3rd year IT student...");

        if (note == null) return;

        int studentId = UserSession.CurrentUser?.Id ?? 0;
        string studentName = UserSession.CurrentUser?.FullName ?? "Student";

        bool success = await _api.ApplyToJobAsync(item.Id, studentId, studentName, note);
        if (success)
        {
            // Reload directly so ApplicationId is retrieved and synced from backend
            await LoadDataAsync();

            await Application.Current.MainPage.DisplayAlert(
                "Application Sent! 🚀",
                $"Your application for {item.RoleTitle} at {item.CompanyName} has been submitted directly to their recruiter dashboard.",
                "OK");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Could not submit application.", "OK");
        }
    }

    private async Task OnAcceptOfferAsync(JobPosting? item)
    {
        if (item == null || item.ApplicationId == null) return;

        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Accept Internship Offer",
            $"Accept offer from {item.CompanyName}?",
            "Yes, Accept!", "Cancel");

        if (!confirm) return;

        bool success = await _api.UpdateApplicationStageAsync(item.ApplicationId.Value, "Accepted");
        if (success)
        {
            await LoadDataAsync();
            await Application.Current.MainPage.DisplayAlert("Offer Accepted! 🎉", $"Your offer with {item.CompanyName} is now Accepted!", "OK");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Could not update status.", "OK");
        }
    }
}