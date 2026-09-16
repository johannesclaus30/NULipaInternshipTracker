using NULipaInternshipTracker.Views;

namespace NULipaInternshipTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("MainPage", typeof(MainPage));
        Routing.RegisterRoute("ApplicationFormPage", typeof(ApplicationFormPage));
        Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
    }
}