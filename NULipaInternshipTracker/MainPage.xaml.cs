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
}