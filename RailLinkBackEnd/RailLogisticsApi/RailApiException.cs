using System.Net;

namespace RailLinkBackEnd.RailLogisticsApi
{
    public class RailApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ResponseBody { get; }

        public RailApiException(HttpStatusCode statusCode, string responseBody)
            : base($"Rail API 호출 실패: {(int)statusCode} {statusCode}")
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }

}
