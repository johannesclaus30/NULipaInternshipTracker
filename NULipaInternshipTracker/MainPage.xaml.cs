using NULipaInternshipTracker.Services;
using NULipaInternshipTracker.ViewModels;

namespace NULipaInternshipTracker;

public partial class MainPage : ContentPage
{
    private readonly InternshipViewModel _vm;
    public MainPage(InternshipViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Render current user photo / initials
        var user = UserSession.CurrentUser;
        if (user != null)
        {
            lblUserInitials.Text = user.Initials;

            if (user.HasProfilePicture)
            {
                imgUserProfile.Source = user.ProfileImageSource;
                imgUserProfile.IsVisible = true;
                lblUserInitials.IsVisible = false;
            }
            else
            {
                imgUserProfile.IsVisible = false;
                lblUserInitials.IsVisible = true;
            }
        }

        await _vm.LoadDataAsync();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => _vm.Filter(e.NewTextValue);

    private void OnFilterChipClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.Parent is HorizontalStackLayout parent)
        {
            foreach (var child in parent.Children.OfType<Button>())
            {
                child.BackgroundColor = Colors.White;
                child.TextColor = Color.FromArgb("#475569");
                child.BorderColor = Color.FromArgb("#E2E8F0");
            }
            btn.BackgroundColor = Color.FromArgb("#32418c");
            btn.TextColor = Colors.White;
            btn.BorderColor = Color.FromArgb("#32418c");
            _vm.SetFilter(btn.CommandParameter?.ToString() ?? "All");
        }
    }

    private async void OnProfileClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("ProfilePage");

    private async void OnMyApplicationsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("MyApplicationsPage");
    }
}