using System.Net;

namespace CKLunchBot;

[Serializable]
public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ApiException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(HttpStatusCode statusCode, string message, Exception inner) : base(message, inner)
    {
        StatusCode = statusCode;
    }
}
