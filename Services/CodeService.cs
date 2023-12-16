using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClassRegisterApp.Models;

namespace ClassRegisterApp.Services;

internal class CodeService
{
    private HttpClient HttpClient { get; } = ApiKeyService.GetConnection();

    internal async Task<Code?> CheckCodeAsync(string code)
    {
        var response =
            await HttpClient.GetAsync($"https://data.mongodb-api.com/app/data-mwqpn/endpoint/code?code={code}");
        if (!response.IsSuccessStatusCode) return null;
        var codeResponse = await response.Content.ReadFromJsonAsync<Code[]>();
        if (codeResponse is { Length: 0 }) return null;
        var currentTime = DateTime.Now;
        return codeResponse?.FirstOrDefault(c => c.Time.AddDays(c.DayExpired) >= currentTime);
    }
}