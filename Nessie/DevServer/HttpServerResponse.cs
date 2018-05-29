using System.Net;
using System.IO;
using MimeTypes;
using System.Text;

namespace Nessie.DevServer
{
    public sealed class HttpServerResponse
    {
        public HttpServerResponse(HttpStatusCode statusCode, WebHeaderCollection headers, Stream body, string contentType)
        {
            StatusCode = statusCode;
            Headers = headers;
            Body = body;
            ContentType = contentType;
        }

        public HttpStatusCode StatusCode { get; }
        public WebHeaderCollection Headers { get; }
        public Stream Body { get; }
        public string ContentType { get; }

        /// <summary>
        /// Create an HTTP response that contains the specified file.
        /// </summary>
        public static HttpServerResponse FileResponse(string filePath)
        {
            return new HttpServerResponse(
                HttpStatusCode.OK,
                new WebHeaderCollection()
                {
                    { "Last-Modified", File.GetLastWriteTime(filePath).ToString("r") }
                },
                new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read),
                MimeTypeMap.GetMimeType(Path.GetExtension(filePath))
            );
        }

        /// <summary>
        /// Create an HTTP response with an html body. Convenience method over StringResponse.
        /// </summary>
        public static HttpServerResponse HtmlResponse(string html, HttpStatusCode status = HttpStatusCode.OK) =>
            StringResponse(html, "text/html", status);

        /// <summary>
        /// Create an HTTP response with a string body.
        /// </summary>
        public static HttpServerResponse StringResponse(string stringResponse, string mimeType = "text/html", HttpStatusCode status = HttpStatusCode.OK)
        {
            return new HttpServerResponse(
                status,
                new WebHeaderCollection(),
                new MemoryStream(Encoding.UTF8.GetBytes(stringResponse ?? "")),
                mimeType
            );
        }
    }
}