using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _api;

    public ProfilePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var user = UserSession.CurrentUser;
        if (user != null)
        {
            entryFullName.Text = user.FullName;
            entryEmail.Text = user.Email;
            lblRoleBadge.Text = $"Role: {user.Role}";
            entryPassword.Text = string.Empty;
        }
    }

    private async void OnSaveChangesClicked(object sender, EventArgs e)
    {
        if (UserSession.CurrentUser == null) return;

        string name = entryFullName.Text?.Trim() ?? string.Empty;
        string email = entryEmail.Text?.Trim() ?? string.Empty;
        string pass = entryPassword.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Validation", "Name and email cannot be empty.", "OK");
            return;
        }

        bool success = await _api.UpdateProfileAsync(UserSession.CurrentUser.Id, name, email, pass);
        if (success)
        {
            UserSession.CurrentUser.FullName = name;
            UserSession.CurrentUser.Email = email;
            if (!string.IsNullOrWhiteSpace(pass))
                UserSession.CurrentUser.Password = pass;

            await DisplayAlert("Success", "Profile details updated successfully!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlert("Error", "Could not update profile. Email might already be taken.", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sign Out", "Are you sure you want to log out?", "Log Out", "Cancel");
        if (!confirm) return;

        UserSession.Clear();
        Application.Current!.MainPage = new LoginPage(_api);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}