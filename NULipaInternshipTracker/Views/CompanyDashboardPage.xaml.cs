using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class CompanyDashboardPage : ContentPage
{
    private readonly ApiService _api;

    public CompanyDashboardPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        lblCompanyName.Text = UserSession.CurrentUser?.FullName ?? "Recruiter Portal";
        await LoadApplicationsAsync();
    }

    private async void OnRefreshRequested(object sender, EventArgs e)
    {
        await LoadApplicationsAsync();
        refreshView.IsRefreshing = false;
    }

    private async Task LoadApplicationsAsync()
    {
        var list = await _api.GetApplicationsByRoleAsync();
        collectionApplicants.ItemsSource = list;
    }

    private async void OnSetInterviewClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is JobApplication app)
        {
            bool ok = await _api.UpdateApplicationStageAsync(app.Id, "Interview");
            if (ok) await LoadApplicationsAsync();
        }
    }

    private async void OnSendOfferClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is JobApplication app)
        {
            bool ok = await _api.UpdateApplicationStageAsync(app.Id, "Offer / MOA");
            if (ok) await LoadApplicationsAsync();
        }
    }

    private async void OnRejectClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is JobApplication app)
        {
            bool confirm = await DisplayAlert("Reject Candidate", $"Mark application for {app.StudentName} as Rejected?", "Yes", "No");
            if (!confirm) return;

            bool ok = await _api.UpdateApplicationStageAsync(app.Id, "Rejected");
            if (ok) await LoadApplicationsAsync();
        }
    }

    private void OnSignOutClicked(object sender, EventArgs e)
    {
        UserSession.Clear();
        Application.Current!.MainPage = new LoginPage(_api);
    }
}