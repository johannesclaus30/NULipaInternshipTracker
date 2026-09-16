using NULipaInternshipTracker.Models;

namespace NULipaInternshipTracker.Services;

public static class UserSession
{
    public static User? CurrentUser { get; set; }

    public static bool IsLoggedIn => CurrentUser != null;
    public static bool IsStudent => CurrentUser?.Role == "Student";
    public static bool IsCompanyAdmin => CurrentUser?.Role == "Company";
    public static bool IsSuperAdmin => CurrentUser?.Role == "SuperAdmin";

    public static void Clear() => CurrentUser = null;
}