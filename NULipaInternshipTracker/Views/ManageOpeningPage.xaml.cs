using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class ManageOpeningPage : ContentPage
{
    private readonly ApiService _api;
    private readonly JobPosting _posting;

    public ManageOpeningPage(ApiService api, JobPosting posting)
    {
        InitializeComponent();
        _api = api;
        _posting = posting;

        lblRoleTitle.Text = _posting.RoleTitle;
        lblPostingMeta.Text = $"{_posting.WorkSetup} • {_posting.Salary} • {_posting.SlotsAvailable} Slots";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Refresh header in case details changed
        lblRoleTitle.Text = _posting.RoleTitle;
        lblPostingMeta.Text = $"{_posting.WorkSetup} • {_posting.Salary} • {_posting.SlotsAvailable} Slots";

        await LoadApplicantsAsync();
    }

    private async Task LoadApplicantsAsync()
    {
        refreshApplicants.IsRefreshing = true;
        try
        {
            var apps = await _api.GetApplicantsForPostingAsync(_posting.Id);
            collectionApplicants.ItemsSource = apps;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            refreshApplicants.IsRefreshing = false;
        }
    }

    private async void OnRefreshRequested(object sender, EventArgs e) => await LoadApplicantsAsync();

    private async void OnSetInterviewClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is JobApplication app)
        {
            bool ok = await _api.UpdateApplicationStageAsync(app.Id, "Interview");
            if (ok) await LoadApplicantsAsync();
        }
    }

    private async void OnSendOfferClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is JobApplication app)
        {
            bool ok = await _api.UpdateApplicationStageAsync(app.Id, "Offer / MOA");
            if (ok) await LoadApplicantsAsync();
        }
    }

    private async void OnRejectClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is JobApplication app)
        {
            bool confirm = await DisplayAlert("Decline", $"Decline {app.StudentName}?", "Decline", "Cancel");
            if (!confirm) return;
            bool ok = await _api.UpdateApplicationStageAsync(app.Id, "Rejected");
            if (ok) await LoadApplicantsAsync();
        }
    }

    private async void OnEditPostingClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PostOpeningPage(_api, _posting));
    }

    private async void OnDeletePostingClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Delete Vacancy", $"Are you sure you want to permanently delete {_posting.RoleTitle}?", "Delete", "Cancel");
        if (!confirm) return;

        bool ok = await _api.DeletePostingAsync(_posting.Id);
        if (ok)
        {
            await DisplayAlert("Deleted", "Vacancy removed.", "OK");
            await Navigation.PopAsync();
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}