using Etrea3.Core;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Etrea3.Networking.API
{
    class ConnectionHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var remoteIP = request.GetClientIPAddress();
            if (BlockManager.Instance.IsIPAddressBanned(remoteIP))
            {
                Game.LogMessage($"WARN: Connection from {remoteIP} will be dropped as the IP address is banned", LogLevel.Warning);
                return Task.FromResult(request.CreateResponse(System.Net.HttpStatusCode.Forbidden, "Your IP address has been banned"));
            }
            var response = base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}
