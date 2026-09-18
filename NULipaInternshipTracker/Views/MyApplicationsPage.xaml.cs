using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.ViewModels;

namespace NULipaInternshipTracker.Views;

public partial class MyApplicationsPage : ContentPage
{
    private readonly MyApplicationsViewModel _vm;

    public MyApplicationsPage(MyApplicationsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadDataAsync();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _vm.Filter(e.NewTextValue);
    }

    private void OnFilterChipClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            if (btn.Parent is HorizontalStackLayout parent)
            {
                foreach (var child in parent.Children.OfType<Button>())
                {
                    child.BackgroundColor = Colors.White;
                    child.TextColor = Color.FromArgb("#475569");
                    child.BorderColor = Color.FromArgb("#E2E8F0");
                }
            }

            btn.BackgroundColor = Color.FromArgb("#32418c");
            btn.TextColor = Colors.White;
            btn.BorderColor = Color.FromArgb("#32418c");

            _vm.SetStageFilter(btn.CommandParameter?.ToString() ?? "All");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnApplicationDetailsClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not JobApplication app) return;

        if (app.IsInterviewStage)
        {
            string date = string.IsNullOrWhiteSpace(app.InterviewDate) ? "TBD (To be determined)" : app.InterviewDate;
            string time = string.IsNullOrWhiteSpace(app.InterviewTime) ? "TBD" : app.InterviewTime;
            string loc = string.IsNullOrWhiteSpace(app.InterviewLocation) ? "Recruiter will reach out via email" : app.InterviewLocation;
            string note = string.IsNullOrWhiteSpace(app.InterviewNotes) ? "None" : app.InterviewNotes;

            await DisplayAlert(
                "📅 Interview Information",
                $"Company: {app.CompanyName}\nPosition: {app.RoleTitle}\n\n" +
                $"• Date: {date}\n" +
                $"• Time: {time}\n" +
                $"• Venue / Link: {loc}\n" +
                $"• Recruiter Instructions: {note}",
                "Got It");
        }
        else if (app.IsWaitlistedStage)
        {
            string note = string.IsNullOrWhiteSpace(app.InterviewNotes)
                ? "Your interview has been completed. Your application is currently under deliberation while remaining candidates are assessed."
                : app.InterviewNotes;

            await DisplayAlert(
                "Application Status: On Hold / Waitlisted",
                $"Company: {app.CompanyName}\nPosition: {app.RoleTitle}\n\n" +
                $"Status: Interview Completed - Under Review\n\n" +
                $"Recruiter Update:\n\"{note}\"",
                "Close");
        }
        else if (app.IsRejectedStage)
        {
            string reason = string.IsNullOrWhiteSpace(app.RejectionReason)
                ? "Thank you for your application. After careful deliberation following this process, other candidates matched the role requirements more closely at this time."
                : app.RejectionReason;

            await DisplayAlert(
                "Application Feedback",
                $"Company: {app.CompanyName}\nPosition: {app.RoleTitle}\n\n" +
                $"Decision: Application Not Selected\n\n" +
                $"Feedback from Recruiter:\n\"{reason}\"",
                "Close");
        }
    }
}