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
}