using System.Text.RegularExpressions;
using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class PostOpeningPage : ContentPage
{
    private readonly ApiService _api;
    private readonly JobPosting? _existingPosting;
    private readonly bool _isEditMode;

    // 1. Constructor for creating new posting
    public PostOpeningPage(ApiService api) : this(api, null)
    {
    }

    // 2. Overloaded constructor for editing existing posting
    public PostOpeningPage(ApiService api, JobPosting? existingPosting)
    {
        InitializeComponent();
        _api = api;
        _existingPosting = existingPosting;
        _isEditMode = existingPosting != null;

        if (_isEditMode && _existingPosting != null)
        {
            lblPageTitle.Text = "Edit Internship Vacancy";
            btnSubmit.Text = "Save Changes";

            // Pre-fill existing data
            entryRole.Text = _existingPosting.RoleTitle;
            pickerSetup.SelectedItem = _existingPosting.WorkSetup;
            entrySlots.Text = _existingPosting.SlotsAvailable.ToString();
            editorDesc.Text = _existingPosting.Description;

            // Pre-fill Allowance
            if (string.Equals(_existingPosting.Salary, "Unpaid", StringComparison.OrdinalIgnoreCase))
            {
                switchAllowance.IsToggled = false;
                layoutSalary.IsVisible = false;
            }
            else
            {
                switchAllowance.IsToggled = true;
                layoutSalary.IsVisible = true;
                // Strip "Php ", "/ day", etc. to get raw numeric value
                string rawSalary = Regex.Replace(_existingPosting.Salary ?? "", @"[^\d]", "");
                entrySalary.Text = rawSalary;
            }
        }
        else
        {
            lblPageTitle.Text = "Post Internship Vacancy";
            btnSubmit.Text = "Publish to Student Feed";
            pickerSetup.SelectedIndex = 0;
            switchAllowance.IsToggled = true;
            layoutSalary.IsVisible = true;
        }
    }

    // Prevents any non-digit character from being typed or pasted into Slots
    private void OnSlotsTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue)) return;

        string digitsOnly = Regex.Replace(e.NewTextValue, @"[^\d]", "");
        if (digitsOnly != e.NewTextValue)
        {
            entrySlots.Text = digitsOnly;
        }
    }

    // Prevents any non-digit character from being typed or pasted into Salary
    private void OnSalaryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue)) return;

        string digitsOnly = Regex.Replace(e.NewTextValue, @"[^\d]", "");
        if (digitsOnly != e.NewTextValue)
        {
            entrySalary.Text = digitsOnly;
        }
    }

    private void OnAllowanceToggled(object sender, ToggledEventArgs e)
    {
        layoutSalary.IsVisible = e.Value;
    }

    private async void OnPublishClicked(object sender, EventArgs e)
    {
        string role = entryRole.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(role))
        {
            await DisplayAlert("Validation", "Please provide a Role Title.", "OK");
            return;
        }

        if (!int.TryParse(entrySlots.Text, out int slots) || slots <= 0)
        {
            await DisplayAlert("Validation", "Please enter a valid positive number of slots.", "OK");
            return;
        }

        string stipendString = "Unpaid";
        if (switchAllowance.IsToggled)
        {
            string amount = entrySalary.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(amount) || amount == "0")
            {
                await DisplayAlert("Validation", "Please enter a daily stipend amount or turn off allowance.", "OK");
                return;
            }
            stipendString = $"Php {amount} / day";
        }

        btnSubmit.IsEnabled = false;

        if (_isEditMode && _existingPosting != null)
        {
            // UPDATE EXISTING VACANCY
            _existingPosting.RoleTitle = role;
            _existingPosting.WorkSetup = pickerSetup.SelectedItem?.ToString() ?? "Onsite";
            _existingPosting.SlotsAvailable = slots;
            _existingPosting.Salary = stipendString;
            _existingPosting.Description = editorDesc.Text?.Trim() ?? string.Empty;

            bool ok = await _api.UpdatePostingAsync(_existingPosting.Id, _existingPosting);
            if (ok)
            {
                await DisplayAlert("Success", "Internship vacancy updated successfully!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "Could not update vacancy. Please try again.", "OK");
            }
        }
        else
        {
            // CREATE NEW VACANCY
            int compId = UserSession.CurrentUser?.CompanyId ?? 1;
            var comp = await _api.GetCompanyByIdAsync(compId);
            string companyName = comp?.Name ?? UserSession.CurrentUser?.FullName ?? "Partner Company";
            string? logo = comp?.Logo;

            var posting = new JobPosting
            {
                CompanyId = compId,
                CompanyName = companyName,
                RoleTitle = role,
                WorkSetup = pickerSetup.SelectedItem?.ToString() ?? "Onsite",
                Salary = stipendString,
                SlotsAvailable = slots,
                Description = editorDesc.Text?.Trim() ?? string.Empty,
                PostedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                CompanyLogo = logo
            };

            bool ok = await _api.CreateJobPostingAsync(posting);
            if (ok)
            {
                await DisplayAlert("Success", "Internship opening published successfully!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "Could not publish opening.", "OK");
            }
        }

        btnSubmit.IsEnabled = true;
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}