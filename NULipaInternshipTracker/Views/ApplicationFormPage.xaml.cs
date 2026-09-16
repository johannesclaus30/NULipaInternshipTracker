using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

[QueryProperty(nameof(AppId), "appId")]
public partial class ApplicationFormPage : ContentPage
{
    private readonly ApiService _api;
    private JobApplication? _existing;
    public int AppId { get; set; } = 0;

    // Holds the Base64 image string (or null)
    private string? _base64Logo = null;
    private const long MaxFileSizeBytes = 1 * 1024 * 1024; // 1 MB Limit

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

                // 👉 1. LOAD THE SAVED NOTES HERE:
                editorNotes.Text = _existing.Notes;

                // Load existing Logo if available
                if (!string.IsNullOrWhiteSpace(_existing.CompanyLogo))
                {
                    _base64Logo = _existing.CompanyLogo;
                    SetLogoPreviewFromBase64(_base64Logo);
                }

                if (_existing.ExpectedSalary == "No Allowance" || _existing.ExpectedSalary == "Unpaid")
                {
                    switchUnpaid.IsToggled = true;
                }
                else
                {
                    switchUnpaid.IsToggled = false;
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

            // 👉 2. CLEAR THE FIELD FOR A FRESH NEW LOG:
            editorNotes.Text = string.Empty;
        }
    }

    private async void OnPickLogoClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Company Logo",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null) return;

            // Enforce reasonable file size limit (1MB)
            var fileInfo = new FileInfo(result.FullPath);
            if (fileInfo.Length > MaxFileSizeBytes)
            {
                await DisplayAlert("File Too Large", "Please select an image smaller than 1 MB.", "OK");
                return;
            }

            // Read into byte array and convert to Base64
            using var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] imageBytes = memoryStream.ToArray();

            _base64Logo = Convert.ToBase64String(imageBytes);

            // Display in Preview
            imgLogoPreview.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            imgLogoPreview.IsVisible = true;
            lblLogoPlaceholder.IsVisible = false;
            btnRemoveLogo.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Picker Error", $"Could not load image: {ex.Message}", "OK");
        }
    }

    private void OnRemoveLogoClicked(object sender, EventArgs e)
    {
        _base64Logo = null;
        imgLogoPreview.Source = null;
        imgLogoPreview.IsVisible = false;
        lblLogoPlaceholder.IsVisible = true;
        btnRemoveLogo.IsVisible = false;
    }

    private void SetLogoPreviewFromBase64(string base64)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(base64);
            imgLogoPreview.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            imgLogoPreview.IsVisible = true;
            lblLogoPlaceholder.IsVisible = false;
            btnRemoveLogo.IsVisible = true;
        }
        catch
        {
            // Fallback to placeholder if corrupt
            imgLogoPreview.IsVisible = false;
            lblLogoPlaceholder.IsVisible = true;
            btnRemoveLogo.IsVisible = false;
        }
    }

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
            Notes = editorNotes?.Text?.Trim() ?? "",
            CompanyLogo = _base64Logo // Sent to your PHP API seamlessly
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