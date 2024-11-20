using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClassRegisterApp.Service;

/// <summary>
/// 
/// </summary>
public static class UserService
{
    private const string Url = "https://chedule-huflit-class.vercel.app/api/user";
    static HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(10)
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="userpw"></param>
    public static async void SaveUser(string user, string userpw)
    {
        await _httpClient.PostAsJsonAsync(Url,
            new UserDKMH(user, userpw),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static async Task<UserDKMH?> GetUser(string userId)
    {
        try
        {
            var httpResponseMessage = await _httpClient.GetAsync(Url + $"?id{userId}");
            return await httpResponseMessage.Content.ReadFromJsonAsync<UserDKMH>(
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        } catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public class UserDKMH
    {
        public UserDKMH(string user, string userPw)
        {
            User = user;
            UserPw = userPw;
        }

        public string User { get; set; }

        public string UserPw { get; set; }
    }
}
