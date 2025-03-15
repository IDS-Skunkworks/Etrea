using Etrea3.Core;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Etrea3.Networking.API
{
    public class APIKeyAuthenticationHandler : DelegatingHandler
    {
        private const string API_KEY_HEADER = "X-API-Key";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var remoteIP = request.GetClientIPAddress();
            if (!request.Headers.Contains(API_KEY_HEADER))
            {
                Game.LogMessage($"WARN: Connection from {remoteIP} did not provide an API key", LogLevel.Warning);
                return Task.FromResult(request.CreateResponse(System.Net.HttpStatusCode.Unauthorized, "API Key was not provided"));
            }
            var apiKey = request.Headers.GetValues(API_KEY_HEADER).FirstOrDefault();
            if (!DatabaseManager.ValidateAPIKey(apiKey, out string player))
            {
                Game.LogMessage($"WARN: Connection from {remoteIP} sent an invalid API key", LogLevel.Warning);
                return Task.FromResult(request.CreateResponse(System.Net.HttpStatusCode.Unauthorized, "Invalid API key"));
            }
            Game.LogMessage($"INFO: Connection from {remoteIP} authenticated as Player {player}", LogLevel.Info);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
