using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _api;
    private bool _isStudentMode = true;
    private bool _isRegistering = false;

    public LoginPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private void OnStudentTabClicked(object sender, EventArgs e)
    {
        _isStudentMode = true;
        btnTabStudent.BackgroundColor = Color.FromArgb("#32418c");
        btnTabStudent.TextColor = Colors.White;

        btnTabAdmin.BackgroundColor = Colors.Transparent;
        btnTabAdmin.TextColor = Color.FromArgb("#64748B");

        lblEmailTitle.Text = "Student Email";
        lblNameTitle.Text = "Full Name";
        entryEmail.Placeholder = "student@nulipa.edu.ph";
        layoutCompanyName.IsVisible = false;
        RefreshUIState();
    }

    private void OnAdminTabClicked(object sender, EventArgs e)
    {
        _isStudentMode = false;
        btnTabAdmin.BackgroundColor = Color.FromArgb("#32418c");
        btnTabAdmin.TextColor = Colors.White;

        btnTabStudent.BackgroundColor = Colors.Transparent;
        btnTabStudent.TextColor = Color.FromArgb("#64748B");

        lblEmailTitle.Text = "Corporate Work Email";
        lblNameTitle.Text = "HR / Recruiter Full Name";
        entryEmail.Placeholder = "recruiter@smprime.com";
        RefreshUIState();
    }

    private void OnToggleRegisterModeClicked(object sender, EventArgs e)
    {
        _isRegistering = !_isRegistering;
        RefreshUIState();
    }

    private void RefreshUIState()
    {
        layoutName.IsVisible = _isRegistering;
        layoutCompanyName.IsVisible = _isRegistering && !_isStudentMode;

        if (_isRegistering)
        {
            btnSubmit.Text = _isStudentMode ? "Create Student Account" : "Register Partner Company";
            btnToggleRegister.Text = "Already have an account? Sign in";
        }
        else
        {
            btnSubmit.Text = "Sign In";
            btnToggleRegister.Text = _isStudentMode ? "Need an account? Register here" : "Register new company account";
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        string email = entryEmail.Text?.Trim() ?? string.Empty;
        string pass = entryPassword.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
        {
            await DisplayAlert("Required", "Please provide both email and password.", "OK");
            return;
        }

        btnSubmit.IsEnabled = false;

        if (_isRegistering)
        {
            string name = entryName.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Required", "Please provide your full name.", "OK");
                btnSubmit.IsEnabled = true;
                return;
            }

            if (_isStudentMode)
            {
                var registered = await _api.RegisterAsync(name, email, pass);
                if (registered != null)
                {
                    UserSession.CurrentUser = registered;
                    Application.Current!.MainPage = new AppShell();
                }
                else
                {
                    await DisplayAlert("Registration Failed", "Email may already be registered.", "OK");
                }
            }
            else
            {
                string companyName = entryCompanyName.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(companyName))
                {
                    await DisplayAlert("Required", "Please provide the registered company name.", "OK");
                    btnSubmit.IsEnabled = true;
                    return;
                }

                var companyUser = await _api.RegisterCompanyAsync(companyName, name, email, pass);
                if (companyUser != null)
                {
                    UserSession.CurrentUser = companyUser;
                    await DisplayAlert("Success", $"Company profile and recruiter account registered for {companyName}.", "Continue");
                    Application.Current!.MainPage = new NavigationPage(new CompanyDashboardPage(_api));
                }
                else
                {
                    await DisplayAlert("Registration Failed", "Failed to register company. Email might already exist.", "OK");
                }
            }
        }
        else
        {
            var user = await _api.LoginAsync(email, pass);
            if (user == null)
            {
                await DisplayAlert("Login Failed", "Invalid credentials.", "OK");
                btnSubmit.IsEnabled = true;
                return;
            }

            UserSession.CurrentUser = user;

            if (user.Role == "Student")
            {
                // Route directly to the main tracking view
                Application.Current!.MainPage = new AppShell();
            }
            else
            {
                Application.Current!.MainPage = new NavigationPage(new CompanyDashboardPage(_api));
            }
        }

        btnSubmit.IsEnabled = true;
    }
}