using NULipaInternshipTracker.Services;
using NULipaInternshipTracker.Views;

namespace NULipaInternshipTracker;

public partial class App : Application
{
    public App(ApiService api)
    {
        InitializeComponent();
        MainPage = new LoginPage(api);
    }
}