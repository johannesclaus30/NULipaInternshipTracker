using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

[QueryProperty(nameof(AppId), "appId")]
public partial class ApplicationFormPage : ContentPage
{
    private readonly ApiService _api;
    private JobApplication? _existing;
    public int AppId { get; set; } = 0;

    public ApplicationFormPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (AppId > 0)
        {
            _existing = await _api.GetByIdAsync(AppId);
            if (_existing != null)
            {
                lblHeader.Text = "Update Internship";
                entryCompany.Text = _existing.CompanyName;
                entryRole.Text = _existing.RoleTitle;
                pickerStage.SelectedItem = _existing.Stage;
                pickerSetup.SelectedItem = _existing.WorkSetup;

                // Parse the Salary logic
                if (_existing.ExpectedSalary == "No Allowance" || _existing.ExpectedSalary == "Unpaid")
                {
                    switchUnpaid.IsToggled = true;
                }
                else
                {
                    switchUnpaid.IsToggled = false;
                    // Strip out "Php " so only the number goes into the Entry
                    entrySalary.Text = _existing.ExpectedSalary.Replace("Php ", "").Replace("Php", "").Trim();
                }

                if (DateTime.TryParse(_existing.ApplicationDate, out var dt))
                    datePickerApp.Date = dt;
            }
        }
        else
        {
            pickerStage.SelectedIndex = 0;
            pickerSetup.SelectedIndex = 0;
            switchUnpaid.IsToggled = false;
        }
    }

    // Hide the numeric entry box when 'No Allowance' is flipped ON
    private void OnUnpaidToggled(object sender, ToggledEventArgs e)
    {
        borderSalary.IsVisible = !e.Value;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(entryCompany.Text) || string.IsNullOrWhiteSpace(entryRole.Text))
        {
            await DisplayAlert("Required Fields", "Company and Role Title must be filled.", "OK");
            return;
        }

        // Format the Salary
        string finalSalary = "No Allowance";
        if (!switchUnpaid.IsToggled)
        {
            finalSalary = string.IsNullOrWhiteSpace(entrySalary.Text) ? "No Allowance" : $"Php {entrySalary.Text.Trim()}";
        }

        var item = new JobApplication
        {
            CompanyName = entryCompany.Text.Trim(),
            RoleTitle = entryRole.Text.Trim(),
            Stage = pickerStage.SelectedItem?.ToString() ?? "Applied",
            WorkSetup = pickerSetup.SelectedItem?.ToString() ?? "Remote",
            ExpectedSalary = finalSalary,
            ApplicationDate = $"{datePickerApp.Date:yyyy-MM-dd}",
            Notes = "" // Kept empty or add editor back if desired
        };

        bool ok;
        if (_existing == null)
            ok = await _api.CreateAsync(item) != null;
        else
        {
            item.Id = _existing.Id;
            ok = await _api.UpdateAsync(item.Id, item);
        }

        if (ok) await Shell.Current.GoToAsync("..");
        else await DisplayAlert("Error", "Failed to save.", "OK");
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}