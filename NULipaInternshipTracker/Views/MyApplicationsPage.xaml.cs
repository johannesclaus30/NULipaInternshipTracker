using NULipaInternshipTracker.ViewModels;

namespace NULipaInternshipTracker.Views;

public partial class MyApplicationsPage : ContentPage
{
    private readonly MyApplicationsViewModel _vm;

    public MyApplicationsPage(MyApplicationsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadDataAsync();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _vm.Filter(e.NewTextValue);
    }

    private void OnFilterChipClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            if (btn.Parent is HorizontalStackLayout parent)
            {
                foreach (var child in parent.Children.OfType<Button>())
                {
                    child.BackgroundColor = Colors.White;
                    child.TextColor = Color.FromArgb("#475569");
                    child.BorderColor = Color.FromArgb("#E2E8F0");
                }
            }

            btn.BackgroundColor = Color.FromArgb("#32418c");
            btn.TextColor = Colors.White;
            btn.BorderColor = Color.FromArgb("#32418c");

            _vm.SetStageFilter(btn.CommandParameter?.ToString() ?? "All");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}