using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nessie.DevServer
{
    /// <summary>
    /// Auto-Refresh for browsers, using the Server Sent Events API
    /// </summary>
    public sealed class AutoRefresh
    {
        public readonly string ServerEndpoint;

        private readonly byte[] JavaScript;
        private readonly StringBuilder ServerSentEvents = new StringBuilder();

        public AutoRefresh()
        {
            this.ServerEndpoint = "/nessie-dev-server-auto-refresh";
            this.JavaScript = Encoding.UTF8.GetBytes($@"
                <script>
                    new EventSource('{ServerEndpoint}').onmessage = function(e) {{
                        window.location.reload();
                    }}
                </script>
            ");
        }

        public async Task KeepAliveLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                this.SendKeepAliveNotification();
            }
        }

        public HttpServerResponse SendPollResponse()
        {
            var response = ServerSentEvents.ToString();
            ServerSentEvents.Clear();
            return HttpServerResponse.StringResponse(response, "text/event-stream");
        }

        public void SendClientRefresh() =>
            ServerSentEvents
                .AppendLine("data: refresh")
                .AppendLine();

        private void SendKeepAliveNotification() =>
            ServerSentEvents
                .AppendLine(":stayin' alive") // ':' is the comment character for SSE
                .AppendLine();

        public async Task AppendAutoRefreshJavaScript(HttpServerResponse response, Stream outputStream)
        {
            if (response.ContentType == "text/html")
            {
                await outputStream.WriteAsync(JavaScript, 0, JavaScript.Length).ConfigureAwait(false);
            }
        }
    }
}
