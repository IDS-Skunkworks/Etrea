using Etrea3.Core;
using System;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Security.Principal;

namespace Etrea3.Networking.API
{
    public class APIServer
    {
        private string baseAddress;
        private HttpSelfHostServer server;
        private bool IsRunning = false;

        public APIServer(string baseAddress = "")
        {
            if (string.IsNullOrEmpty(baseAddress))
            {
                this.baseAddress = ConfigurationManager.AppSettings["APIUrl"];
            }
            else
            {
                this.baseAddress = baseAddress;
            }
        }

        public void Init()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        Game.LogMessage($"WARN: MUD Server must be run as Administrator to start the API", LogLevel.Warning);
                        return;
                    }
                }
                var config = new HttpSelfHostConfiguration(baseAddress);
                config.MessageHandlers.Add(new ConnectionHandler());
                config.MessageHandlers.Add(new APIKeyAuthenticationHandler());
                config.MessageHandlers.Add(new LoggingHandler());
                config.Routes.MapHttpRoute(name: "GetResourceByID", routeTemplate: "api/{controller}/{id}", defaults: new { id = RouteParameter.Optional });
                config.Routes.MapHttpRoute(name: "GetResourceByName", routeTemplate: "api/{controller}/name/{name}", defaults: new { name = RouteParameter.Optional });
                server = new HttpSelfHostServer(config);
                server.OpenAsync().Wait();
                IsRunning = true;
                Game.LogMessage($"INFO: API server started successfully on {baseAddress}", LogLevel.Info);
            }
            catch(Exception ex)
            {
                Game.LogMessage($"ERROR: Error starting API server: {ex.Message} ({ex.HResult})", LogLevel.Error);
            }
        }

        public bool Stop()
        {
            try
            {
                if (IsRunning)
                {
                    server.CloseAsync().Wait();
                    server.Dispose();
                    IsRunning = false;
                    Game.LogMessage($"INFO: API server stopped successfully", LogLevel.Info);
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error stopping API server: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
    }
}
