using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly ApiService _api;
    private bool _isStudentMode = true;

    public ForgotPasswordPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private void OnTabStudentClicked(object sender, EventArgs e)
    {
        _isStudentMode = true;
        btnTabStudent.BackgroundColor = Color.FromArgb("#32418c");
        btnTabStudent.TextColor = Colors.White;
        btnTabCompany.BackgroundColor = Colors.Transparent;
        btnTabCompany.TextColor = Color.FromArgb("#475569");

        layoutStudentForm.IsVisible = true;
        layoutCompanyForm.IsVisible = false;
    }

    private void OnTabCompanyClicked(object sender, EventArgs e)
    {
        _isStudentMode = false;
        btnTabCompany.BackgroundColor = Color.FromArgb("#32418c");
        btnTabCompany.TextColor = Colors.White;
        btnTabStudent.BackgroundColor = Colors.Transparent;
        btnTabStudent.TextColor = Color.FromArgb("#475569");

        layoutCompanyForm.IsVisible = true;
        layoutStudentForm.IsVisible = false;
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        string newPass = entryNewPassword.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(newPass) || newPass.Length < 6)
        {
            await DisplayAlert("Validation", "Please provide a valid password of at least 6 characters.", "OK");
            return;
        }

        bool ok = false;
        if (_isStudentMode)
        {
            string name = entryStudentName.Text?.Trim() ?? string.Empty;
            string email = entryStudentEmail.Text?.Trim() ?? string.Empty;
            string snum = entryStudentNo.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(snum))
            {
                await DisplayAlert("Validation", "Please fill in all student verification fields.", "OK");
                return;
            }

            ok = await _api.ForgotPasswordStudentAsync(name, email, snum, newPass);
        }
        else
        {
            string cname = entryCompanyName.Text?.Trim() ?? string.Empty;
            string rname = entryRecruiterName.Text?.Trim() ?? string.Empty;
            string email = entryCompanyEmail.Text?.Trim() ?? string.Empty;
            string code = entryCompanyCode.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cname) || string.IsNullOrWhiteSpace(rname) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                await DisplayAlert("Validation", "Please fill in all recruiter verification fields.", "OK");
                return;
            }

            ok = await _api.ForgotPasswordCompanyAsync(cname, rname, email, code, newPass);
        }

        if (ok)
        {
            await DisplayAlert("Password Reset Successful", "Your password has been updated securely. You can now log in.", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Verification Failed", "The information entered does not match any registered records. Please review and try again.", "OK");
        }
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ForgotPasswordPage(_api));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}