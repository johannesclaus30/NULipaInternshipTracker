using System.Text.Json.Serialization;

namespace NULipaInternshipTracker.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Student"; // "Student" or "CompanyAdmin"
    public int? CompanyId { get; set; }
    public string? StudentNumber { get; set; }
    public string? CompanyCode { get; set; }
    public string? ProfilePicture { get; set; } // Base64 encoded JPEG/PNG

    [JsonIgnore]
    public bool HasProfilePicture => !string.IsNullOrWhiteSpace(ProfilePicture);

    [JsonIgnore]
    public ImageSource? ProfileImageSource
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ProfilePicture)) return null;
            try
            {
                byte[] bytes = Convert.FromBase64String(ProfilePicture);
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch
            {
                return null;
            }
        }
    }

    [JsonIgnore]
    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FullName)) return "NU";
            var parts = FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }
    }
}