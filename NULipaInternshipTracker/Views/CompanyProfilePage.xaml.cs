using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class CompanyProfilePage : ContentPage
{
    private readonly ApiService _api;
    private string? _base64Logo = null;
    private Company? _company = null;

    public CompanyProfilePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var user = UserSession.CurrentUser;
        if (user != null)
        {
            entryRecruiterName.Text = user.FullName;
            entryEmail.Text = user.Email;

            // Load saved company entity data from MySQL
            if (user.CompanyId.HasValue)
            {
                _company = await _api.GetCompanyByIdAsync(user.CompanyId.Value);
                if (_company != null)
                {
                    entryCompanyName.Text = _company.Name;
                    if (!string.IsNullOrWhiteSpace(_company.Logo))
                    {
                        _base64Logo = _company.Logo;
                        SetLogoPreview(_base64Logo);
                    }
                }
            }
        }
    }

    private void SetLogoPreview(string base64)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(base64);
            imgLogoPreview.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            imgLogoPreview.IsVisible = true;
            lblLogoPlaceholder.IsVisible = false;
        }
        catch
        {
            imgLogoPreview.IsVisible = false;
            lblLogoPlaceholder.IsVisible = true;
        }
    }

    private void OnTabCompanyClicked(object sender, EventArgs e)
    {
        btnTabCompany.BackgroundColor = Color.FromArgb("#32418c");
        btnTabCompany.TextColor = Colors.White;
        btnTabRecruiter.BackgroundColor = Colors.Transparent;
        btnTabRecruiter.TextColor = Color.FromArgb("#475569");

        layoutCompanyTab.IsVisible = true;
        layoutRecruiterTab.IsVisible = false;
    }

    private void OnTabRecruiterClicked(object sender, EventArgs e)
    {
        btnTabRecruiter.BackgroundColor = Color.FromArgb("#32418c");
        btnTabRecruiter.TextColor = Colors.White;
        btnTabCompany.BackgroundColor = Colors.Transparent;
        btnTabCompany.TextColor = Color.FromArgb("#475569");

        layoutCompanyTab.IsVisible = false;
        layoutRecruiterTab.IsVisible = true;
    }

    private async void OnUploadLogoClicked(object sender, EventArgs e)
    {
        try
        {
            var res = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Company Logo",
                FileTypes = FilePickerFileType.Images
            });
            if (res == null) return;

            using var stream = await res.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] bytes = ms.ToArray();

            _base64Logo = Convert.ToBase64String(bytes);
            SetLogoPreview(_base64Logo);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load image: {ex.Message}", "OK");
        }
    }

    private void OnRemoveLogoClicked(object sender, EventArgs e)
    {
        _base64Logo = null;
        imgLogoPreview.Source = null;
        imgLogoPreview.IsVisible = false;
        lblLogoPlaceholder.IsVisible = true;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var user = UserSession.CurrentUser;
        if (user == null) return;

        string cName = entryCompanyName.Text?.Trim() ?? string.Empty;
        string rName = entryRecruiterName.Text?.Trim() ?? string.Empty;
        string email = entryEmail.Text?.Trim() ?? string.Empty;
        string pass = entryPassword.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cName) || string.IsNullOrWhiteSpace(rName) || string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Validation", "Company name, recruiter name, and email are required.", "OK");
            return;
        }

        int compId = user.CompanyId ?? 0;
        bool ok = await _api.UpdateCompanyProfileAsync(compId, user.Id, cName, _base64Logo, rName, email, pass);

        if (ok)
        {
            user.FullName = rName;
            user.Email = email;
            await DisplayAlert("Success", "Settings and company logo updated successfully!", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Could not update settings.", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}