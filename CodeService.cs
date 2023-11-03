using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ClassRegisterApp;

internal class CodeService
{
    private HttpClient HttpClient { get; } = new();

    internal async Task<bool> CheckCode(string code)
    {
        var response =
            await HttpClient.GetAsync($"https://data.mongodb-api.com/app/data-mwqpn/endpoint/code?code={code}");
        var codeResponse = await response.Content.ReadFromJsonAsync<Code[]>();
        if (codeResponse is { Length: 0 }) return false;


        var currentTime = DateTime.Now;
        return codeResponse != null && codeResponse.Any(c => c.Time.AddDays(c.DayExpired) >= currentTime);
    }
}