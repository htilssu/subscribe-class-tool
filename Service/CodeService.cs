using System.Threading.Tasks;
using ClassRegisterApp.Model;

namespace ClassRegisterApp.Service;

internal class CodeService
{
    public static async Task<RequestResult<Code>> VerifyCodeAsync(string code)
    {
        return await RequestService.PostAsync<Code>("/v1/code/verify", new { code });
    }
}
