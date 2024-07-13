using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClassRegisterApp.Models;

namespace ClassRegisterApp.Services;

public static class SecretService
{
    private const string Url = "https://chedule-huflit-class.vercel.app/api/secret";
    static HttpClient _httpClient = new();

    public static async Task<Class?> GetSecret(string classId)
    {
        var response = await _httpClient.GetAsync(Url + "?classId=" + classId);
        if (!response.IsSuccessStatusCode) return null;
        try
        {
            var cClass = await response.Content.ReadFromJsonAsync<Class>();
            return cClass;
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }
    
    public static async Task<Class[]> GetAllSecrets()
    {
        var response = await _httpClient.GetAsync(Url);
        if (!response.IsSuccessStatusCode) return [];
        try
        {
            var cClasses = await response.Content.ReadFromJsonAsync<Class[]>();
            return cClasses ?? [];
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return [];
    }

    public static async Task<bool> AddSecret(Class @class)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(Url, @class);
            return response.IsSuccessStatusCode;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }

    public static Class[]? ConvertDicToClass(Dictionary<string, string> baseClass
        , Dictionary<string, Dictionary<string, string>> childClass)
    {
        List<Class> classes = [];
        if (baseClass.Count == 0) return null;
        foreach (var keyValuePair in baseClass)
        {
            var @class = new Class(keyValuePair.Key, keyValuePair.Value);
            if (!childClass.TryGetValue(keyValuePair.Key, out var value)) continue;
            foreach (var child in value)
            {
                @class.AddChild(new Class(child.Key, child.Value));
            }

            classes.Add(@class);
        }

        return classes.ToArray();
    }
}
