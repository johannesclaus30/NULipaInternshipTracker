using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class ManageOpeningPage : ContentPage
{
    private readonly ApiService _api;
    private readonly JobPosting _posting;
    private JobApplication? _selectedAppForSchedule;

    public ManageOpeningPage(ApiService api, JobPosting posting)
    {
        InitializeComponent();
        _api = api;
        _posting = posting;

        // Set calendar minimum date safely in C#
        pickerInterviewDate.MinimumDate = DateTime.Today;

        lblRoleTitle.Text = _posting.RoleTitle;
        lblPostingMeta.Text = $"{_posting.WorkSetup} • {_posting.Salary} • {_posting.SlotsAvailable} Slots";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

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
            Console.WriteLine($"Error loading applicants: {ex.Message}");
        }
        finally
        {
            refreshApplicants.IsRefreshing = false;
        }
    }

    private async void OnRefreshRequested(object sender, EventArgs e) => await LoadApplicantsAsync();

    private void OnSetInterviewClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not JobApplication app) return;

        _selectedAppForSchedule = app;
        lblModalStudentName.Text = $"Candidate: {app.StudentName}";

        if (DateTime.TryParse(app.InterviewDate, out DateTime parsedDate) && parsedDate >= DateTime.Today)
            pickerInterviewDate.Date = parsedDate;
        else
            pickerInterviewDate.Date = DateTime.Today.AddDays(1);

        if (TimeSpan.TryParse(app.InterviewTime, out TimeSpan parsedTime))
            pickerInterviewTime.Time = parsedTime;
        else
            pickerInterviewTime.Time = new TimeSpan(10, 0, 0); // 10:00 AM default

        entryModalLocation.Text = string.IsNullOrWhiteSpace(app.InterviewLocation)
            ? "Google Meet / Microsoft Teams"
            : app.InterviewLocation;

        editorModalNotes.Text = string.IsNullOrWhiteSpace(app.InterviewNotes)
            ? "Please bring an updated copy of your CV and portfolio."
            : app.InterviewNotes;

        layoutScheduleModal.IsVisible = true;
    }

    private void OnCancelScheduleModalClicked(object sender, EventArgs e)
    {
        layoutScheduleModal.IsVisible = false;
        _selectedAppForSchedule = null;
    }

    private async void OnConfirmScheduleModalClicked(object sender, EventArgs e)
    {
        if (_selectedAppForSchedule == null) return;

        DateTime selectedDate = pickerInterviewDate.Date ?? DateTime.Today.AddDays(1);
        string dateString = $"{selectedDate.Year:D4}-{selectedDate.Month:D2}-{selectedDate.Day:D2}";

        TimeSpan rawTime = pickerInterviewTime.Time ?? new TimeSpan(10, 0, 0);
        DateTime dt = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, rawTime.Hours, rawTime.Minutes, 0);
        string timeString = dt.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);

        string location = entryModalLocation.Text?.Trim() ?? string.Empty;
        string notes = editorModalNotes.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(location))
        {
            await DisplayAlert("Validation", "Please specify an interview venue or online meeting link.", "OK");
            return;
        }

        bool ok = await _api.UpdateApplicationStageWithDetailsAsync(
            _selectedAppForSchedule.Id,
            "Interview",
            dateString,
            timeString,
            location,
            notes);

        layoutScheduleModal.IsVisible = false;

        if (ok)
        {
            await DisplayAlert("Scheduled! 📅", $"Interview scheduled for {_selectedAppForSchedule.StudentName}.", "OK");
            await LoadApplicantsAsync();
        }
        else
        {
            await DisplayAlert("Error", "Could not save interview schedule.", "OK");
        }

        _selectedAppForSchedule = null;
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
        if (sender is not Button btn || btn.CommandParameter is not JobApplication app) return;

        string action = await DisplayActionSheet(
            $"Decline {app.StudentName}?",
            "Cancel",
            null,
            "Slots already filled",
            "Qualifications / Tech stack mismatch",
            "Schedule conflict / Unavailability",
            "Custom reason...");

        if (action == "Cancel" || string.IsNullOrEmpty(action)) return;

        string rejectionReason = action;
        if (action == "Custom reason...")
        {
            string custom = await DisplayPromptAsync("Feedback for Applicant", "Enter rejection note/constructive feedback:");
            if (string.IsNullOrWhiteSpace(custom)) return;
            rejectionReason = custom.Trim();
        }

        bool ok = await _api.UpdateApplicationStageWithDetailsAsync(
            app.Id,
            "Rejected",
            rejectionReason: rejectionReason);

        if (ok)
        {
            await DisplayAlert("Application Updated", "Feedback recorded and sent to applicant.", "OK");
            await LoadApplicantsAsync();
        }
        else
        {
            await DisplayAlert("Error", "Could not update application status.", "OK");
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

    private async void OnInterviewCompletedClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not JobApplication app) return;

        string decision = await DisplayActionSheet(
            $"Evaluation Decision for {app.StudentName}",
            "Cancel",
            null,
            "🎉 Extend Internship Offer",
            "⏳ Put on Waitlist / On Hold",
            "❌ Decline / Reject");

        if (decision == "Cancel" || string.IsNullOrEmpty(decision)) return;

        if (decision == "🎉 Extend Internship Offer")
        {
            bool confirm = await DisplayAlert(
                "Send Formal Offer",
                $"Extend an internship offer to {app.StudentName} for {_posting.RoleTitle}?",
                "Extend Offer", "Cancel");

            if (!confirm) return;

            bool ok = await _api.UpdateApplicationStageAsync(app.Id, "Offer / MOA");
            if (ok)
            {
                await DisplayAlert("Success", "Internship offer extended to candidate!", "OK");
                await LoadApplicantsAsync();
            }
        }
        else if (decision == "⏳ Put on Waitlist / On Hold")
        {
            string note = await DisplayPromptAsync(
                "Waitlist Candidate",
                "Provide an optional status note for the student (e.g., 'Awaiting final batch review'):",
                initialValue: "Under deliberation after interview. Final decision pending next round of interviews.");

            if (note == null) return; // User canceled prompt

            bool ok = await _api.UpdateApplicationStageWithDetailsAsync(
                app.Id,
                "Waitlisted",
                interviewNotes: note.Trim());

            if (ok)
            {
                await DisplayAlert("Candidate Waitlisted", $"{app.StudentName} marked as On Hold / Waitlisted.", "OK");
                await LoadApplicantsAsync();
            }
        }
        else if (decision == "❌ Decline / Reject")
        {
            // Re-use your reject feedback workflow
            OnRejectClicked(sender, e);
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}