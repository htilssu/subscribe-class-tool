using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClassRegisterApp.Models;

namespace ClassRegisterApp.Services;

internal class CodeService
{
    internal async Task<Code?> CheckCodeAsync(string code)
    {
        var response = await DataService.SendRequest(HttpMethod.Get, $"/api/v1/code?code={code}", null);
        if (!response.IsSuccessStatusCode) return null;
        try
        {
            var codeResponse = await response.Content.ReadFromJsonAsync<Code[]>();
            if (codeResponse is { Length: 0 }) return null;
            var currentTime = DateTime.Now;
            return codeResponse?.FirstOrDefault(c => c.Time.AddDays(c.DayExpired) >= currentTime);
        } catch (Exception e) { Console.WriteLine(e); }

        return null;
    }
}
