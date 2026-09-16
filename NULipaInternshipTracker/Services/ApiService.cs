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

    public async Task<List<JobApplication>> GetAllAsync()
    {
        try
        {
            var resp = await _client.GetAsync("applications/get_all.php");
            resp.EnsureSuccessStatusCode();
            string json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<JobApplication>>(json, _opts) ?? new List<JobApplication>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Error: {ex.Message}");
            return new List<JobApplication>();
        }
    }

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

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var resp = await _client.DeleteAsync($"applications/delete.php?id={id}");
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // Inside Services/ApiService.cs

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

    public async Task<User?> RegisterAsync(string fullName, string email, string password)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { FullName = fullName, Email = email, Password = password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("auth/register.php", content);

            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<User>(json, _opts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Register error: {ex.Message}");
            return null;
        }
    }

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

    public async Task<bool> UpdateApplicationStageAsync(int appId, string stage)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { Stage = stage });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PutAsync($"applications/update.php?id={appId}", content);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<User?> RegisterCompanyAsync(string companyName, string contactPerson, string email, string password)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                CompanyName = companyName,
                ContactPerson = contactPerson,
                Email = email,
                Password = password
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("auth/register_company.php", content);

            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<User>(json, _opts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Company registration error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateProfileAsync(int userId, string fullName, string email, string? newPassword)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                Id = userId,
                FullName = fullName,
                Email = email,
                Password = string.IsNullOrWhiteSpace(newPassword) ? null : newPassword
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("auth/update_profile.php", content);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Profile update error: {ex.Message}");
            return false;
        }
    }
}