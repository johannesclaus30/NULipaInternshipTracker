using NULipaInternshipTracker.Models;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _api;
    private bool _isRegisterMode = false;
    private bool _isStudentRole = true;

    public LoginPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        UpdateViewMode();
    }

    private void OnStudentTabClicked(object sender, EventArgs e)
    {
        _isStudentRole = true;
        btnTabStudent.BackgroundColor = Color.FromArgb("#32418c");
        btnTabStudent.TextColor = Colors.White;
        btnTabAdmin.BackgroundColor = Colors.Transparent;
        btnTabAdmin.TextColor = Color.FromArgb("#64748B");

        lblEmailTitle.Text = "Student Email *";
        entryEmail.Placeholder = "student@nulipa.edu.ph";
        lblNameTitle.Text = "Full Name *";
        UpdateViewMode();
    }

    private void OnAdminTabClicked(object sender, EventArgs e)
    {
        _isStudentRole = false;
        btnTabAdmin.BackgroundColor = Color.FromArgb("#32418c");
        btnTabAdmin.TextColor = Colors.White;
        btnTabStudent.BackgroundColor = Colors.Transparent;
        btnTabStudent.TextColor = Color.FromArgb("#64748B");

        lblEmailTitle.Text = "Corporate Work Email *";
        entryEmail.Placeholder = "recruiter@company.com";
        lblNameTitle.Text = "Recruiter Full Name *";
        UpdateViewMode();
    }

    private void OnToggleRegisterModeClicked(object sender, EventArgs e)
    {
        _isRegisterMode = !_isRegisterMode;
        UpdateViewMode();
    }

    private void UpdateViewMode()
    {
        if (_isRegisterMode)
        {
            btnSubmit.Text = "Create Account";
            btnToggleRegister.Text = "Already have an account? Sign in here";
            btnForgotPassword.IsVisible = false;

            layoutName.IsVisible = true;
            layoutStudentId.IsVisible = _isStudentRole;
            layoutCompanyName.IsVisible = !_isStudentRole;
            layoutCompanyCode.IsVisible = !_isStudentRole;
        }
        else
        {
            btnSubmit.Text = "Sign In";
            btnToggleRegister.Text = "Need an account? Register here";
            btnForgotPassword.IsVisible = true;

            layoutName.IsVisible = false;
            layoutStudentId.IsVisible = false;
            layoutCompanyName.IsVisible = false;
            layoutCompanyCode.IsVisible = false;
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        string email = entryEmail.Text?.Trim() ?? string.Empty;
        string password = entryPassword.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Validation", "Email and Password are required.", "OK");
            return;
        }

        if (_isRegisterMode)
        {
            string name = entryName.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Validation", "Full name is required.", "OK");
                return;
            }

            if (_isStudentRole)
            {
                string studentId = entryStudentId.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    await DisplayAlert("Validation", "Student ID number is required.", "OK");
                    return;
                }

                var studentUser = await _api.RegisterStudentAsync(name, email, password, studentId);
                if (studentUser != null)
                {
                    UserSession.CurrentUser = studentUser;
                    Application.Current!.MainPage = new AppShell();
                }
                else
                {
                    await DisplayAlert("Registration Failed", "Could not create account. Email may already be taken.", "OK");
                }
            }
            else // Company Host
            {
                string companyName = entryCompanyName.Text?.Trim() ?? string.Empty;
                string companyCode = entryCompanyCode.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(companyCode))
                {
                    await DisplayAlert("Validation", "Company Name and Company ID are required.", "OK");
                    return;
                }

                var companyUser = await _api.RegisterCompanyAsync(companyName, companyCode, name, email, password);
                if (companyUser != null)
                {
                    UserSession.CurrentUser = companyUser;
                    Application.Current!.MainPage = new NavigationPage(new CompanyDashboardPage(_api));
                }
                else
                {
                    await DisplayAlert("Registration Failed", "Could not register company account.", "OK");
                }
            }
        }
        else // Sign In Mode
        {
            var user = await _api.LoginAsync(email, password);
            if (user != null)
            {
                UserSession.CurrentUser = user;

                if (user.Role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    Application.Current!.MainPage = new AppShell();
                }
                else
                {
                    Application.Current!.MainPage = new NavigationPage(new CompanyDashboardPage(_api));
                }
            }
            else
            {
                await DisplayAlert("Sign In Failed", "Invalid email or password.", "OK");
            }
        }
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new ForgotPasswordPage(_api));
    }
}