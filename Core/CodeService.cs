using System.Threading.Tasks;
using ClassRegisterApp.Models;
using ClassRegisterApp.Infrastructure;

namespace ClassRegisterApp.Core;

internal class CodeService
{
    public static async Task<RequestResult<Code>> VerifyCodeAsync(string code)
    {
        var result = await RequestService.PostAsync<Code>("/v1/code/verify", new { code });
        if (result.IsOk && result.Result != null)
        {
            await CodeStorageService.SaveCodeAsync(result.Result);
        }
        return result;
    }

    public static async Task<Code?> LoadSavedCodeAsync()
    {
        return await CodeStorageService.LoadCodeAsync();
    }
}
