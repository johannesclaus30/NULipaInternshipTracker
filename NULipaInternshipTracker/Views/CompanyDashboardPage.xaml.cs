using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class CompanyDashboardPage : ContentPage
{
    private readonly ApiService _api;
    private List<JobPosting> _openings = new();

    public CompanyDashboardPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        refreshView.IsRefreshing = true;
        try
        {
            int compId = UserSession.CurrentUser?.CompanyId ?? 0;
            if (compId > 0)
            {
                _openings = await _api.GetCompanyPostingsAsync(compId);
                collectionOpenings.ItemsSource = _openings;

                // Load all applicants for this company for dashboard analytics
                var allApps = await _api.GetApplicationsByRoleAsync();

                lblStatOpenings.Text = _openings.Count.ToString();
                lblStatApplicants.Text = allApps.Count.ToString();
                lblStatInterview.Text = allApps.Count(a => a.Stage == "Interview" || a.Stage == "For Interview").ToString();
                lblStatHired.Text = allApps.Count(a => a.Stage == "Accepted").ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dashboard error: {ex.Message}");
        }
        finally
        {
            refreshView.IsRefreshing = false;
        }
    }

    private async void OnRefreshRequested(object sender, EventArgs e) => await LoadDataAsync();

    private async void OnManageOpeningClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is JobPosting posting)
        {
            await Navigation.PushAsync(new ManageOpeningPage(_api, posting));
        }
    }

    private async void OnPostOpeningClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PostOpeningPage(_api));
    }

    private async void OnCompanySettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CompanyProfilePage(_api));
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sign Out", "Are you sure you want to sign out?", "Sign Out", "Cancel");
        if (!confirm) return;

        UserSession.Clear();
        Microsoft.Maui.Controls.Application.Current!.MainPage = new LoginPage(_api);
    }
}