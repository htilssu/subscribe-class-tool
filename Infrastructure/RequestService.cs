using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClassRegisterApp.Models;
using Newtonsoft.Json;

namespace ClassRegisterApp.Infrastructure;

public class RequestService
{
    private const string BaseUrl = "https://xeplich.htilssu.id.vn";
    private static TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(1);

    public static async Task<RequestResult<T>> GetAsync<T>(string endpoint) where T : class
    {
        var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(endpoint);
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Request failed endpoint: {endpoint}, status code: {response.StatusCode}");
                return RequestResult.Fail<T>(HttpStatusCode.BadRequest);
            }

            var result = await response.Content.ReadFromJsonAsync<T>();

            return result == null ? RequestResult.Fail<T>(HttpStatusCode.BadRequest) : RequestResult.Ok(result);
        } catch (JsonException e)
        {
            Console.WriteLine(e.Message);
            return RequestResult.Fail<T>(HttpStatusCode.BadRequest);
        }
    }

    public static async Task<RequestResult<T>> PostAsync<T>(string endpoint, object data) where T : class
    {
        var httpClient = GetHttpClient();
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await httpClient.PostAsync(endpoint, content);
        try
        {
            if (!response.IsSuccessStatusCode) return RequestResult.Fail<T>(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<T>();

            return result == null ? RequestResult.Fail<T>(HttpStatusCode.BadRequest) : RequestResult.Ok(result);

        } catch (JsonException e)
        {
            Console.WriteLine(e.Message);
            return RequestResult.Fail<T>(HttpStatusCode.BadRequest);
        }
    }

    private static HttpClient GetHttpClient()
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.Timeout = TimeOut;

        return httpClient;
    }
}
