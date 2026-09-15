namespace NULipaInternshipTracker.Views;

public partial class WelcomePage : ContentPage
{
    public WelcomePage()
    {
        InitializeComponent();
    }

    private async void OnOpenTrackerClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("MainPage");
    }
}