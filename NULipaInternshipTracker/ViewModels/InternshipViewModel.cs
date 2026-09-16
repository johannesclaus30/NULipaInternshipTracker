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
    private readonly List<JobApplication> _masterList = new();

    public ObservableCollection<JobApplication> Applications { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; Notify(); }
    }

    private int _totalApplied;
    public int TotalApplied
    {
        get => _totalApplied;
        set { _totalApplied = value; Notify(); }
    }

    private int _interviewsSecured;
    public int InterviewsSecured
    {
        get => _interviewsSecured;
        set { _interviewsSecured = value; Notify(); }
    }

    private string _currentSearchQuery = string.Empty;
    private string _selectedStage = "All";

    // Commands
    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public InternshipViewModel(ApiService api)
    {
        _api = api;

        RefreshCommand = new Command(async () => await LoadDataAsync());
        AddCommand = new Command(async () => await Shell.Current.GoToAsync("ApplicationFormPage"));
        EditCommand = new Command<JobApplication>(async (item) =>
        {
            if (item != null)
                await Shell.Current.GoToAsync($"ApplicationFormPage?appId={item.Id}");
        });
        DeleteCommand = new Command<JobApplication>(async (item) => await ConfirmDeleteAsync(item));
    }

    public async Task LoadDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var list = await _api.GetAllAsync(); // or your ApiService method name

            _masterList.Clear();
            if (list != null)
            {
                _masterList.AddRange(list);
            }

            UpdateStats();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Inside ViewModels/InternshipViewModel.cs

    private int _offersAccepted;
    public int OffersAccepted
    {
        get => _offersAccepted;
        set { _offersAccepted = value; Notify(); }
    }

    private void UpdateStats()
    {
        TotalApplied = _masterList.Count;

        InterviewsSecured = _masterList.Count(a =>
            !string.IsNullOrEmpty(a.Stage) &&
            a.Stage.Equals("Interview", StringComparison.OrdinalIgnoreCase));

        // Count accepted / offer stage
        OffersAccepted = _masterList.Count(a =>
            !string.IsNullOrEmpty(a.Stage) &&
            (a.Stage.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ||
             a.Stage.Equals("Offer / MOA", StringComparison.OrdinalIgnoreCase)));
    }

    public void Filter(string query)
    {
        _currentSearchQuery = query ?? string.Empty;
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

        // 1. Filter by Stage Chip
        if (!string.Equals(_selectedStage, "All", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(a =>
                !string.IsNullOrEmpty(a.Stage) &&
                a.Stage.Equals(_selectedStage, StringComparison.OrdinalIgnoreCase));
        }

        // 2. Filter by Search Query
        if (!string.IsNullOrWhiteSpace(_currentSearchQuery))
        {
            filtered = filtered.Where(a =>
                (a.CompanyName != null && a.CompanyName.Contains(_currentSearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                (a.RoleTitle != null && a.RoleTitle.Contains(_currentSearchQuery, StringComparison.OrdinalIgnoreCase)));
        }

        Applications.Clear();
        foreach (var app in filtered)
        {
            Applications.Add(app);
        }
    }

    private async Task ConfirmDeleteAsync(JobApplication? item)
    {
        if (item == null) return;

        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Delete Application",
            $"Are you sure you want to remove your log for {item.CompanyName}?",
            "Yes", "No");

        if (!confirm) return;

        bool success = await _api.DeleteAsync(item.Id);
        if (success)
        {
            _masterList.Remove(item);
            Applications.Remove(item);
            UpdateStats();
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete record.", "OK");
        }
    }
}