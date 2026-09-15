using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.ViewModels;

public class InternshipViewModel : INotifyPropertyChanged
{
    private readonly ApiService _api;
    private List<JobApplication> _rawList = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    void Notify(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    public ObservableCollection<JobApplication> Applications { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; Notify(nameof(IsBusy)); }
    }

    private string _statusMessage = "Loading records...";
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; Notify(nameof(StatusMessage)); }
    }

    private int _totalApplied;
    public int TotalApplied
    {
        get => _totalApplied;
        set { _totalApplied = value; Notify(nameof(TotalApplied)); }
    }

    private int _interviewsSecured;
    public int InterviewsSecured
    {
        get => _interviewsSecured;
        set { _interviewsSecured = value; Notify(nameof(InterviewsSecured)); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public InternshipViewModel(ApiService api)
    {
        _api = api;
        RefreshCommand = new Command(async () => await LoadDataAsync());
        AddCommand = new Command(async () => await Shell.Current.GoToAsync("ApplicationFormPage"));
        EditCommand = new Command<JobApplication>(async item =>
        {
            if (item != null)
                await Shell.Current.GoToAsync($"ApplicationFormPage?appId={item.Id}");
        });
        DeleteCommand = new Command<JobApplication>(async item => await ConfirmDeleteAsync(item));
    }

    public async Task LoadDataAsync()
    {
        IsBusy = true;
        StatusMessage = "Fetching applications...";

        _rawList = await _api.GetAllAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Applications.Clear();
            foreach (var item in _rawList)
            {
                Applications.Add(item);
            }

            TotalApplied = _rawList.Count;
            InterviewsSecured = _rawList.Count(x => !string.IsNullOrEmpty(x.Stage) && x.Stage.Equals("Interview", StringComparison.OrdinalIgnoreCase));
            StatusMessage = _rawList.Count > 0 ? $"{_rawList.Count} applications tracked." : "No internship applications logged yet.";
            IsBusy = false;
        });
    }

    public void Filter(string query)
    {
        Applications.Clear();
        var matches = string.IsNullOrWhiteSpace(query)
            ? _rawList
            : _rawList.Where(x => x.CompanyName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                  x.RoleTitle.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach (var item in matches) Applications.Add(item);
    }

    private async Task ConfirmDeleteAsync(JobApplication item)
    {
        if (item == null) return;
        bool confirmed = await Shell.Current.DisplayAlert("Confirm Removal", $"Delete application for {item.CompanyName}?", "Yes", "No");
        if (!confirmed) return;

        bool success = await _api.DeleteAsync(item.Id);
        if (success)
        {
            Applications.Remove(item);
            _rawList.Remove(item);
            TotalApplied = _rawList.Count;
            InterviewsSecured = _rawList.Count(x => x.Stage.Equals("Interview", StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Could not delete record from server.", "OK");
        }
    }
}