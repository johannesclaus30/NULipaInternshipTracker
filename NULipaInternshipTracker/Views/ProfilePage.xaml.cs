using Microsoft.Maui.Graphics.Platform;
using NULipaInternshipTracker.Services;

namespace NULipaInternshipTracker.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _api;
    private string? _base64ProfilePhoto;

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
            entryName.Text = user.FullName;
            entryEmail.Text = user.Email;
            entryPassword.Text = string.Empty;
            entryStudentId.Text = user.StudentNumber ?? string.Empty;

            _base64ProfilePhoto = user.ProfilePicture;
            RenderAvatar(_base64ProfilePhoto);
        }
    }

    private void RenderAvatar(string? base64)
    {
        if (!string.IsNullOrWhiteSpace(base64))
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                imgProfileAvatar.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                imgProfileAvatar.IsVisible = true;
                lblAvatarPlaceholder.IsVisible = false;
                return;
            }
            catch
            {
                // Fall back if decoding fails
            }
        }

        imgProfileAvatar.IsVisible = false;
        lblAvatarPlaceholder.IsVisible = true;
    }

    private async void OnChangePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var res = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Profile Photo",
                FileTypes = FilePickerFileType.Images
            });

            if (res == null) return;

            using var stream = await res.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] rawBytes = ms.ToArray();

            // 🗜️ Compress and downsize camera photo to ~30-50KB
            byte[] compressedBytes = ResizeAndCompressImage(rawBytes, maxDimension: 300);

            _base64ProfilePhoto = Convert.ToBase64String(compressedBytes);
            RenderAvatar(_base64ProfilePhoto);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load photo: {ex.Message}", "OK");
        }
    }

    private void OnRemovePhotoClicked(object sender, EventArgs e)
    {
        _base64ProfilePhoto = null;
        RenderAvatar(null);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var user = UserSession.CurrentUser;
        if (user == null) return;

        string name = entryName.Text?.Trim() ?? string.Empty;
        string email = entryEmail.Text?.Trim() ?? string.Empty;
        string pass = entryPassword.Text?.Trim() ?? string.Empty;
        string studentId = entryStudentId.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Validation", "Name and email are required.", "OK");
            return;
        }

        var updatedUser = await _api.UpdateProfileAsync(user.Id, name, email, studentId, pass, _base64ProfilePhoto);
        if (updatedUser != null)
        {
            UserSession.CurrentUser = updatedUser;
            await DisplayAlert("Success", "Profile updated successfully!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlert("Error", "Could not update profile.", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sign Out", "Are you sure you want to sign out?", "Sign Out", "Cancel");
        if (!confirm) return;

        UserSession.Clear();
        Microsoft.Maui.Controls.Application.Current!.MainPage = new LoginPage(_api);
    }

    private byte[] ResizeAndCompressImage(byte[] originalBytes, int maxDimension = 300)
    {
        try
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS
            var image = PlatformImage.FromStream(new MemoryStream(originalBytes));
            if (image != null)
            {
                // Calculate proportional dimensions (max 300px)
                float width = image.Width;
                float height = image.Height;

                if (width > maxDimension || height > maxDimension)
                {
                    if (width > height)
                    {
                        height = (height / width) * maxDimension;
                        width = maxDimension;
                    }
                    else
                    {
                        width = (width / height) * maxDimension;
                        height = maxDimension;
                    }
                }

                // Downscale image
                var downsizedImage = image.Downsize(width, height, true);
                using var ms = new MemoryStream();
                // Compress as JPEG at 75% quality
                downsizedImage.Save(ms, ImageFormat.Jpeg, 0.75f);
                return ms.ToArray();
            }
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Image compression warning: {ex.Message}");
        }

        return originalBytes;
    }
}