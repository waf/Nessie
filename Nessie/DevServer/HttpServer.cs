/*
 */
using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Nessie.Services.Utils;

namespace Nessie.DevServer
{
    /// <summary>
    /// Simple <see cref="HttpListener" />-based server for local development.
    /// We should really use Kestrel here instead, but Kestrel doesn't work well with
    /// AOT compilation (see open issue https://github.com/aspnet/Home/issues/3079)
    /// </summary>
    public sealed class HttpServer : IDisposable
    {
        private const string IndexFile = "index.html";

        private HttpListener listener;
        private CancellationTokenSource cancellationToken;
        private readonly string path;
        private readonly string host;
        private readonly int port;
        private readonly AutoRefresh autoRefresher;

        public HttpServer(string path, string host, int port)
        {
            this.path = path.NormalizeDirectorySeparators();
            this.host = host;
            this.port = port;
            this.autoRefresher = new AutoRefresh();
        }

        public void Start()
        {
            if (cancellationToken != null) throw new InvalidOperationException("HttpServer already started.");
            cancellationToken = new CancellationTokenSource();

            Task.Run(() => Listen(cancellationToken.Token));
            Task.Run(() => autoRefresher.KeepAliveLoop(cancellationToken.Token));
        }

        private async Task Listen(CancellationToken token)
        {
            // start listener
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(string.Format("http://{0}:{1}/", host, port));
                listener.Start();
            }
            catch (Exception e)
            {
                ReportError(e);
                return;
            }

            // wait for requests
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var context = await listener.GetContextAsync().ConfigureAwait(false);
                    _ = Task.Run(async () =>
                    {
                        try { await ProcessContext(context, token).ConfigureAwait(false); }
                        catch (Exception e) { ReportError(e); }
                    });
                }
                catch (Exception e)
                {
                    ReportError(e);
                    return;
                }
            }
        }

        /// <summary>
        /// Process the received context from the <see cref="HttpListener"/>.
        /// The context represents a single request/response.
        /// </summary>
        private async Task ProcessContext(HttpListenerContext context, CancellationToken token)
        {
            HttpServerResponse response;
            try
            {
                response = GenerateResponse(context);
            }
            catch (Exception e)
            {
                ReportError(e);
                response = HttpServerResponse.HtmlResponse("ERROR: " + e.Message, HttpStatusCode.InternalServerError);
            }

            if(!token.IsCancellationRequested)
            {
                await StreamResponseAsync(context, response, token).ConfigureAwait(false);
            }
        }

        private HttpServerResponse GenerateResponse(HttpListenerContext context)
        {
            string url = context.Request.Url.AbsolutePath;

            if(url == autoRefresher.ServerEndpoint)
            {
                return autoRefresher.SendPollResponse();
                // don't log refresh polling to the console on purpose, because it's too noisy.
            }

            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - HTTP {context.Request.HttpMethod} {url}");
            string filename = WebUtility.UrlDecode(url.TrimStart('/'));

            string exactFile = Path.Combine(path, filename);
            if(File.Exists(exactFile))
            {
                return HttpServerResponse.FileResponse(exactFile);
            }

            string defaultIndexFile = Path.Combine(path, filename, IndexFile);
            if (File.Exists(defaultIndexFile))
            {
                return HttpServerResponse.FileResponse(defaultIndexFile);
            }

            if (Directory.Exists(exactFile))
            {
                var listing = GenerateDirectoryListing(exactFile);
                return HttpServerResponse.HtmlResponse(listing);
            }

            return HttpServerResponse.HtmlResponse("Not Found", HttpStatusCode.NotFound);
        }

        private async Task StreamResponseAsync(HttpListenerContext context, HttpServerResponse response, CancellationToken token)
        {
            Stream outputStream = context.Response.OutputStream;

            context.Response.StatusCode = (int)response.StatusCode;
            context.Response.ContentType = response.ContentType + "; charset=utf-8";
            context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
            using (response.Body)
            {
                await response.Body.CopyToAsync(outputStream, 81920, token).ConfigureAwait(false);
                await autoRefresher.AppendAutoRefreshJavaScript(response, outputStream, token).ConfigureAwait(false);
                await outputStream.FlushAsync(token).ConfigureAwait(false);
            }
            outputStream.Close();
        }

        private string GenerateDirectoryListing(string directory)
        {
            var entries = Directory
                .GetFileSystemEntries(directory)
                .Select(file =>
                {
                    string relativePath = file.NormalizeDirectorySeparators().Replace(path, "");
                    string display = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                    return $"<li><a href='{relativePath}'>{display}</a></li>";
                });
            return $"{DirectoryListingHeader}<ul>{String.Concat(entries)}</ul>";
        }

        /// <summary>
        /// Tell all connected clients to refresh the page
        /// </summary>
        public void SendClientRefresh() =>
            autoRefresher.SendClientRefresh();

        public void Dispose()
        {
            cancellationToken.Cancel();
            if(listener == null)
            {
                return;
            }
            if (listener.IsListening)
            {
                listener.Stop();
            }
            listener.Close();
            listener = null;
        }

        private static void ReportError(Exception e, [CallerMemberName] string caller = null) =>
            Console.Error.WriteLine($"ERROR ({caller}): {e.Message}");

        // hard-coded css is the best css.
        private const string DirectoryListingHeader = @"
            <style>
            body { font-family: sans-serif; }
            h1 { font-size: 16pt; }
            ul { padding: 0; }
            li {
              font-size: 14pt;
              list-style-type: none;
              padding: 5px 20px;
            }
            li:nth-child(odd) { background-color: #f8f8f8; }
            a { color: #0086c1; }
            </style>
            <h1>Directory Listing</h1>
        ";
    }
}