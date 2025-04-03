using System.Net;

namespace ClassRegisterApp.Models;

public class RequestResult<T> where T : class
{
    public T? Result { get; set; }
    public HttpStatusCode Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsOk
    {
        get => Status == HttpStatusCode.OK;
    }
}

public class RequestResult
{
    public static RequestResult<T> Ok<T>(T result) where T : class
    {
        return new RequestResult<T>
        {
            Result = result,
            Status = HttpStatusCode.OK
        };
    }

    public static RequestResult<T> Fail<T>(HttpStatusCode status) where T : class
    {
        return new RequestResult<T>
        {
            Result = null,
            Status = status
        };
    }
}
