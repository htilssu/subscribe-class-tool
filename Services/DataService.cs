using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClassRegisterApp.Services;

public class DataService
{
    private static HttpClient _httpClient = ApiKeyService.GetConnection();
    private const string Url = "https://ap-southeast-1.aws.data.mongodb-api.com/app/application-0-zthuk/endpoint";

    public static async Task<HttpResponseMessage> SendRequest(HttpMethod method, string url, string? body)
    {
        var request = new HttpRequestMessage(method, Url + url);
        if (method == HttpMethod.Post && body != null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return await _httpClient.SendAsync(request);
    }
}

internal static class ApiKeyService
{
    public static HttpClient GetConnection()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("APIKEY", "mCieF690kZtsFoohB5iT3RCLW0omfTgYVPnD7a4EKxY3mefAhhB1GYGQ6pUjJfWx");
        return client;
    }
}
