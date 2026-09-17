using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.ViewModels;

public class MyApplicationsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Notify([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private readonly ApiService _api;
    private readonly List<JobApplication> _masterList = new();

    public ObservableCollection<JobApplication> Applications { get; } = new();

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { _isBusy = value; Notify(); } }

    private int _totalCount;
    public int TotalCount { get => _totalCount; set { _totalCount = value; Notify(); } }

    private int _interviewCount;
    public int InterviewCount { get => _interviewCount; set { _interviewCount = value; Notify(); } }

    private int _offerCount;
    public int OfferCount { get => _offerCount; set { _offerCount = value; Notify(); } }

    private int _acceptedCount;
    public int AcceptedCount { get => _acceptedCount; set { _acceptedCount = value; Notify(); } }

    private string _currentSearch = string.Empty;
    private string _selectedStage = "All";

    public ICommand RefreshCommand { get; }
    public ICommand AcceptOfferCommand { get; }
    public ICommand CancelApplicationCommand { get; }

    public MyApplicationsViewModel(ApiService api)
    {
        _api = api;
        RefreshCommand = new Command(async () => await LoadDataAsync());
        AcceptOfferCommand = new Command<JobApplication>(async (item) => await OnAcceptOfferAsync(item));
        CancelApplicationCommand = new Command<JobApplication>(async (item) => await OnCancelApplicationAsync(item));
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var list = await _api.GetApplicationsByRoleAsync();
            _masterList.Clear();
            if (list != null) _masterList.AddRange(list);

            UpdateStats();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading applications: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateStats()
    {
        TotalCount = _masterList.Count;

        InterviewCount = _masterList.Count(a =>
            !string.IsNullOrEmpty(a.Stage) &&
            (a.Stage.Equals("Interview", StringComparison.OrdinalIgnoreCase) ||
             a.Stage.Equals("For Interview", StringComparison.OrdinalIgnoreCase)));

        OfferCount = _masterList.Count(a =>
            !string.IsNullOrEmpty(a.Stage) &&
            (a.Stage.Equals("Offer", StringComparison.OrdinalIgnoreCase) ||
             a.Stage.Equals("Offer / MOA", StringComparison.OrdinalIgnoreCase)));

        AcceptedCount = _masterList.Count(a =>
            !string.IsNullOrEmpty(a.Stage) &&
            a.Stage.Equals("Accepted", StringComparison.OrdinalIgnoreCase));
    }

    public void Filter(string query)
    {
        _currentSearch = query ?? string.Empty;
        ApplyFilters();
    }

    public void SetStageFilter(string stage)
    {
        _selectedStage = string.IsNullOrWhiteSpace(stage) ? "All" : stage;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _masterList.AsEnumerable();

        if (!string.Equals(_selectedStage, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (_selectedStage.Equals("For Interview", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(a => !string.IsNullOrEmpty(a.Stage) &&
                    (a.Stage.Equals("For Interview", StringComparison.OrdinalIgnoreCase) ||
                     a.Stage.Equals("Interview", StringComparison.OrdinalIgnoreCase)));
            }
            else if (_selectedStage.Equals("Offer / MOA", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(a => !string.IsNullOrEmpty(a.Stage) &&
                    (a.Stage.Equals("Offer", StringComparison.OrdinalIgnoreCase) ||
                     a.Stage.Equals("Offer / MOA", StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                filtered = filtered.Where(a => !string.IsNullOrEmpty(a.Stage) &&
                    a.Stage.Equals(_selectedStage, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (!string.IsNullOrWhiteSpace(_currentSearch))
        {
            filtered = filtered.Where(a =>
                (a.CompanyName != null && a.CompanyName.Contains(_currentSearch, StringComparison.OrdinalIgnoreCase)) ||
                (a.RoleTitle != null && a.RoleTitle.Contains(_currentSearch, StringComparison.OrdinalIgnoreCase)));
        }

        Applications.Clear();
        foreach (var app in filtered)
        {
            Applications.Add(app);
        }
    }

    private async Task OnAcceptOfferAsync(JobApplication? item)
    {
        if (item == null) return;

        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Accept Offer",
            $"Accept internship offer from {item.CompanyName}?",
            "Yes, Accept!", "Cancel");

        if (!confirm) return;

        bool ok = await _api.UpdateApplicationStageAsync(item.Id, "Accepted");
        if (ok)
        {
            item.Stage = "Accepted";
            UpdateStats();
            ApplyFilters();
            await Application.Current.MainPage.DisplayAlert("Congratulations! 🎉", $"You have accepted the offer with {item.CompanyName}.", "OK");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Could not accept offer.", "OK");
        }
    }

    private async Task OnCancelApplicationAsync(JobApplication? item)
    {
        if (item == null) return;

        if (item.Stage.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
        {
            await Application.Current!.MainPage!.DisplayAlert("Notice", "You have already accepted this offer. Please contact the company recruiter directly if you need to withdraw.", "OK");
            return;
        }

        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Withdraw Application",
            $"Are you sure you want to withdraw your application for {item.RoleTitle} at {item.CompanyName}?",
            "Withdraw", "Keep Application");

        if (!confirm) return;

        int studentId = UserSession.CurrentUser?.Id ?? 0;
        bool ok = await _api.CancelApplicationAsync(item.Id, studentId);
        if (ok)
        {
            await LoadDataAsync();
            await Application.Current.MainPage.DisplayAlert("Withdrawn", "Your application has been canceled.", "OK");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Could not cancel application.", "OK");
        }
    }
}