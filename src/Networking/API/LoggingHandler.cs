using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.ServiceModel.Channels;
using Etrea3.Core;
using System.Linq;

namespace Etrea3.Networking.API
{
    public class LoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var remoteIP = request.GetClientIPAddress() ?? "Unknown";
            var routeData = request.GetRouteData();
            var controller = routeData?.Values["controller"]?.ToString() ?? "Unknown Controller";
            var action = routeData?.Values["action"]?.ToString() ?? "Unknown Action";
            Game.LogMessage($"INFO: API received a {request.Method} message from {remoteIP} to {request.RequestUri}", LogLevel.Info, true);
            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }

    public static class Extensions
    {
        public static string GetClientIPAddress(this HttpRequestMessage request)
        {
            if (request.Headers.Contains("X-Forwarded-For"))
            {
                if (request.Headers.TryGetValues("X-Forwarded-For", out var xForward))
                {
                    return xForward.FirstOrDefault() ?? "Unknown";
                }
            }
            if (request.Properties.TryGetValue(RemoteEndpointMessageProperty.Name, out var property))
            {
                var remoteEndpoint = property as RemoteEndpointMessageProperty;
                return remoteEndpoint?.Address;
            }
            return null;
        }
    }
}
