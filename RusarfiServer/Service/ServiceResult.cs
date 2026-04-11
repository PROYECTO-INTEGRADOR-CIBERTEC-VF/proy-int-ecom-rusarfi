namespace RusarfiServer.Service;

public sealed record ServiceResult<T>(bool Success, string Message, T? Data, int StatusCode)
{
    public static ServiceResult<T> Ok(string message, T? data, int statusCode = 200)
        => new(true, message, data, statusCode);

    public static ServiceResult<T> Fail(string message, int statusCode)
        => new(false, message, default, statusCode);
}
