using System;
using System.IO;
using System.Threading.Tasks;
using ClassRegisterApp.Models;

namespace ClassRegisterApp.Infrastructure;

public static class CodeStorageService
{
    private static readonly string StoragePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ClassRegisterApp",
        "code.json"
    );

    public static async Task SaveCodeAsync(Code code)
    {
        try
        {
            var directory = Path.GetDirectoryName(StoragePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(code);
            await File.WriteAllTextAsync(StoragePath, json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Lỗi khi lưu code: {e.Message}");
        }
    }

    public static async Task<Code?> LoadCodeAsync()
    {
        try
        {
            if (!File.Exists(StoragePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(StoragePath);
            return System.Text.Json.JsonSerializer.Deserialize<Code>(json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Lỗi khi đọc code: {e.Message}");
            return null;
        }
    }
} 