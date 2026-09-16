using NULipaInternshipTracker.ViewModels;

namespace NULipaInternshipTracker;

public partial class MainPage : ContentPage
{
    private InternshipViewModel? _vm;

    public MainPage(InternshipViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await EnsureDataLoaded();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await EnsureDataLoaded();
    }

    private async Task EnsureDataLoaded()
    {
        // If Shell created MainPage using the empty constructor, resolve the VM manually
        if (_vm == null)
        {
            _vm = Handler?.MauiContext?.Services.GetService<InternshipViewModel>()
                  ?? IPlatformApplication.Current?.Services.GetService<InternshipViewModel>();

            if (_vm != null)
            {
                BindingContext = _vm;
            }
        }

        if (_vm != null)
        {
            await _vm.LoadDataAsync();
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _vm?.Filter(e.NewTextValue);
    }

    private async void OnBackToProfileClicked(object sender, EventArgs e)
    {
        // Navigates back to the WelcomePage
        await Shell.Current.GoToAsync("..");
    }

    private void OnFilterChipClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedBtn && clickedBtn.CommandParameter is string stage)
        {
            // Highlight selected button, un-highlight siblings
            if (clickedBtn.Parent is HorizontalStackLayout stack)
            {
                foreach (var child in stack.Children.OfType<Button>())
                {
                    child.BackgroundColor = Color.FromArgb("#FFFFFF");
                    child.TextColor = Color.FromArgb("#475569");
                }
                clickedBtn.BackgroundColor = Color.FromArgb("#32418c");
                clickedBtn.TextColor = Color.FromArgb("#FFFFFF");
            }

            // Apply filter to ViewModel
            _vm?.SetStageFilter(stage);
        }
    }

    // Inside MainPage.xaml.cs

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (ApplicationsCollection?.ItemsLayout is GridItemsLayout gridLayout)
        {
            // For standard phone width (< 500), keep it strictly 2 portrait cards per row
            // On tablets or desktop windows (> 500), scale up to 3 or 4 columns so cards don't look stretched
            if (width > 800)
                gridLayout.Span = 4;
            else if (width > 500)
                gridLayout.Span = 3;
            else
                gridLayout.Span = 2;
        }
    }

    private async void OnViewFullNotesClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Models.JobApplication app)
        {
            if (!string.IsNullOrWhiteSpace(app.Notes))
            {
                await DisplayAlert(
                    $"{app.CompanyName} — Notes",
                    app.Notes,
                    "Close"
                );
            }
        }
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("ProfilePage");
    }
}