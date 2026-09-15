using NULipaInternshipTracker.Services;
using NULipaInternshipTracker.ViewModels;
using NULipaInternshipTracker.Views;

namespace NULipaInternshipTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                // Montserrat Family
                fonts.AddFont("montserrat.regular.ttf", "MontserratRegular");
                fonts.AddFont("montserrat.medium.ttf", "MontserratMedium");
                fonts.AddFont("montserrat.semibold.ttf", "MontserratSemiBold");
                fonts.AddFont("montserrat.bold.ttf", "MontserratBold");
                fonts.AddFont("montserrat.extrabold.ttf", "MontserratExtraBold");
                fonts.AddFont("montserrat.black.ttf", "MontserratBlack");
                fonts.AddFont("montserrat.light.ttf", "MontserratLight");
                fonts.AddFont("montserrat.italic.ttf", "MontserratItalic");
            });

        builder.Services.AddSingleton(sp => new HttpClient
        {
#if ANDROID
            BaseAddress = new Uri("http://10.0.2.2/careerapi/"),
#else
            BaseAddress = new Uri("http://127.0.0.1/careerapi/"),
#endif
            Timeout = TimeSpan.FromSeconds(30)
        });

        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddTransient<InternshipViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ApplicationFormPage>();

        return builder.Build();
    }
}