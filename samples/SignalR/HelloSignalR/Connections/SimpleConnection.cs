using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HelloSignalR.Connections {
    public class SimpleConnection : PersistentConnection {
        protected override async Task OnConnected(HttpRequest request, string connectionId) {
            var identity = request.HttpContext.User.Identity;
            var status = identity.IsAuthenticated ? "Authenticated" : "Unauthenticated";

            Logger.LogInformation($"{status} connection {connectionId} has just connected.");

            await Connection.Send(connectionId, $"Connection is {status}");

            if (identity.IsAuthenticated) {
                await Connection.Send(connectionId, $"Authenticated username: {identity.Name}");
            }
        }

        protected override async Task OnReceived(HttpRequest request, string connectionId, string data) {
            var identity = request.HttpContext.User.Identity;
            var status = identity.IsAuthenticated ? "authenticated" : "unauthenticated";
            var name = identity.IsAuthenticated ? identity.Name : "client";

            await Connection.Send(connectionId, $"Received an {status} message from {name}: {data}");
        }
    }
}
