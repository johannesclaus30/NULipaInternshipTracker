using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NULipaInternshipTracker.Models;

namespace NULipaInternshipTracker.Services;

public class ApiService
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public ApiService(HttpClient client)
    {
        _client = client;
    }

    // ==========================================
    // AUTHENTICATION & USER MANAGEMENT
    // ==========================================

    public async Task<User?> LoginAsync(string email, string password)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { Email = email, Password = password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("auth/login.php", content);

            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<User>(json, _opts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return null;
        }
    }

    public async Task<User?> RegisterStudentAsync(string fullName, string email, string password, string studentNumber)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                FullName = fullName,
                Email = email,
                Password = password,
                StudentNumber = studentNumber,
                Role = "Student"
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("auth/register.php", content);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<User>(json, _opts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Student register error: {ex.Message}");
            return null;
        }
    }

    public async Task<User?> RegisterCompanyAsync(string companyName, string companyCode, string recruiterName, string email, string password)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                CompanyName = companyName,
                CompanyCode = companyCode,
                FullName = recruiterName,
                Email = email,
                Password = password,
                Role = "CompanyAdmin"
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("auth/register_company.php", content);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<User>(json, _opts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Company register error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> ForgotPasswordStudentAsync(string fullName, string email, string studentNumber, string newPassword)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                UserType = "Student",
                FullName = fullName,
                Email = email,
                StudentNumber = studentNumber,
                NewPassword = newPassword
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("auth/forgot_password.php", content);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Forgot password error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ForgotPasswordCompanyAsync(string companyName, string recruiterName, string email, string companyCode, string newPassword)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                UserType = "Company",
                CompanyName = companyName,
                RecruiterName = recruiterName,
                Email = email,
                CompanyCode = companyCode,
                NewPassword = newPassword
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("auth/forgot_password.php", content);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Forgot password error: {ex.Message}");
            return false;
        }
    }

    public async Task<User?> UpdateProfileAsync(int userId, string fullName, string email, string? studentNumber, string? newPassword, string? profilePictureBase64 = null)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                Id = userId,
                FullName = fullName,
                Email = email,
                StudentNumber = studentNumber,
                Password = string.IsNullOrWhiteSpace(newPassword) ? null : newPassword,
                ProfilePicture = profilePictureBase64
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("auth/update_profile.php", content);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("user", out var userElem))
            {
                return JsonSerializer.Deserialize<User>(userElem.GetRawText(), _opts);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Profile update error: {ex.Message}");
            return null;
        }
    }

    // ==========================================
    // COMPANIES
    // ==========================================

    public async Task<List<Company>> GetCompaniesAsync()
    {
        try
        {
            var resp = await _client.GetAsync("companies/get_all.php");
            if (!resp.IsSuccessStatusCode) return new();

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Company>>(json, _opts) ?? new();
        }
        catch { return new(); }
    }

    public async Task<Company?> GetCompanyByIdAsync(int id)
    {
        try
        {
            var resp = await _client.GetAsync($"companies/get_one.php?id={id}");
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Company>(json, _opts);
        }
        catch { return null; }
    }

    public async Task<bool> UpdateCompanyProfileAsync(int companyId, int userId, string companyName, string companyCode, string? logoBase64, string recruiterName, string email, string? password)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                CompanyId = companyId,
                UserId = userId,
                CompanyName = companyName,
                CompanyCode = companyCode,
                Logo = logoBase64,
                RecruiterName = recruiterName,
                Email = email,
                Password = string.IsNullOrWhiteSpace(password) ? null : password
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("companies/update_company.php", content);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating company profile: {ex.Message}");
            return false;
        }
    }

    // ==========================================
    // JOB POSTINGS
    // ==========================================

    public async Task<List<JobPosting>> GetJobPostingsAsync(int studentId)
    {
        try
        {
            var resp = await _client.GetAsync($"postings/get_all.php?student_id={studentId}");
            if (!resp.IsSuccessStatusCode) return new();

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<JobPosting>>(json, _opts) ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching postings: {ex.Message}");
            return new();
        }
    }

    public async Task<List<JobPosting>> GetCompanyPostingsAsync(int companyId)
    {
        try
        {
            var resp = await _client.GetAsync($"postings/get_by_company.php?company_id={companyId}");
            if (!resp.IsSuccessStatusCode) return new();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<JobPosting>>(json, _opts) ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching company postings: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> CreateJobPostingAsync(JobPosting posting)
    {
        try
        {
            var payload = JsonSerializer.Serialize(posting);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("postings/create.php", content);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating posting: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdatePostingAsync(int id, JobPosting posting)
    {
        try
        {
            var json = JsonSerializer.Serialize(posting);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync($"postings/update.php?id={id}", content);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> DeletePostingAsync(int id)
    {
        try
        {
            var resp = await _client.GetAsync($"postings/delete.php?id={id}");
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<List<JobApplication>> GetApplicantsForPostingAsync(int postingId)
    {
        try
        {
            var resp = await _client.GetAsync($"postings/get_one_with_applicants.php?id={postingId}");
            if (!resp.IsSuccessStatusCode) return new();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Applicants", out var appsElem))
            {
                return JsonSerializer.Deserialize<List<JobApplication>>(appsElem.GetRawText(), _opts) ?? new();
            }
            return new();
        }
        catch { return new(); }
    }

    // ==========================================
    // APPLICATIONS
    // ==========================================

    public async Task<JobApplication?> GetByIdAsync(int id)
    {
        try
        {
            var resp = await _client.GetAsync($"applications/get_one.php?id={id}");
            if (!resp.IsSuccessStatusCode) return null;
            string json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JobApplication>(json, _opts);
        }
        catch { return null; }
    }

    public async Task<JobApplication?> CreateAsync(JobApplication item)
    {
        try
        {
            string json = JsonSerializer.Serialize(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("applications/create.php", content);
            resp.EnsureSuccessStatusCode();
            string body = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JobApplication>(body, _opts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Create Error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(int id, JobApplication item)
    {
        try
        {
            string json = JsonSerializer.Serialize(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _client.PutAsync($"applications/update.php?id={id}", content);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<List<JobApplication>> GetApplicationsByRoleAsync()
    {
        try
        {
            string url = "applications/get_all.php";

            if (UserSession.IsStudent && UserSession.CurrentUser != null)
                url += $"?student_id={UserSession.CurrentUser.Id}";
            else if (UserSession.IsCompanyAdmin && UserSession.CurrentUser?.CompanyId != null)
                url += $"?company_id={UserSession.CurrentUser.CompanyId}";

            var resp = await _client.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return new();

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<JobApplication>>(json, _opts) ?? new();
        }
        catch { return new(); }
    }

    public async Task<bool> ApplyToJobAsync(int jobPostingId, int studentId, string studentName, string coverNote)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                JobPostingId = jobPostingId,
                StudentId = studentId,
                StudentName = studentName,
                CoverNote = coverNote
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("postings/apply.php", content);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateApplicationStageAsync(int appId, string stage)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { Id = appId, Stage = stage });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("applications/update_stage.php", content);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> UpdateApplicationStageWithDetailsAsync(
        int applicationId,
        string stage,
        string? interviewDate = null,
        string? interviewTime = null,
        string? interviewLocation = null,
        string? interviewNotes = null,
        string? rejectionReason = null)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                Id = applicationId,
                Stage = stage,
                InterviewDate = interviewDate,
                InterviewTime = interviewTime,
                InterviewLocation = interviewLocation,
                InterviewNotes = interviewNotes,
                RejectionReason = rejectionReason
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("applications/update_stage.php", content);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating stage details: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CancelApplicationAsync(int applicationId, int studentId)
    {
        try
        {
            var resp = await _client.DeleteAsync($"applications/cancel.php?id={applicationId}&student_id={studentId}");
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error canceling application: {ex.Message}");
            return false;
        }
    }
}