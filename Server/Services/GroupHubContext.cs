using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Online.Services;
using Fishbowl.Net.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Server.Services
{
    public interface IGroupHubContext : IAsyncDisposable
    {
        int Count { get; }

        bool ContainsKey(string username);

        IGameClient Group();

        IGameClient GroupExcept(IEnumerable<string> usernames);

        IGameClient Client(string username);

        IGameClient Clients(IEnumerable<string> usernames);

        Task RegisterConnection(string username, string connectionId);

        Task RemoveConnection(string connectionId);
    }

    public class GroupHubContext : IGroupHubContext
    {
        public int Count => this.usernameConnectionMap.Count;

        private static readonly IGameClient NullClient = new NullGameClient();

        private readonly Dictionary<string, string?> usernameConnectionMap = new();

        private readonly IHubContext<GameHub, IGameClient> hubContext;

        private readonly string password;

        private readonly ILogger<GroupHubContext> logger;

        public GroupHubContext(
            IHubContext<GameHub, IGameClient> hubContext,
            string password,
            ILogger<GroupHubContext> logger) =>
            (this.hubContext, this.password, this.logger) =
            (hubContext, password, logger);

        public bool ContainsKey(string username) => this.usernameConnectionMap.ContainsKey(username);

        public IGameClient Client(string username)
        {
            var connectionId = this.usernameConnectionMap[username];

            return (connectionId is null) ? NullClient : this.hubContext.Clients.Clients(connectionId);
        }

        public IGameClient Clients(IEnumerable<string> usernames) =>
            this.hubContext.Clients.Clients(this.GetConnections(usernames));

        public IGameClient Group() => this.hubContext.Clients.Group(this.password);

        public IGameClient GroupExcept(IEnumerable<string> usernames) =>
            this.hubContext.Clients.GroupExcept(this.password, this.GetConnections(usernames));

        public Task RegisterConnection(string username, string connectionId)
        {
            this.usernameConnectionMap[username] = connectionId;

            return this.hubContext.Groups.AddToGroupAsync(connectionId, this.password);
        }

        public Task RemoveConnection(string connectionId)
        {
            var entries = this.usernameConnectionMap.Where(entry => entry.Value == connectionId).ToList();

            if (entries.Count < 1)
            {
                this.logger.LogInformation("RemoveConnection: {ConnectionId} not found", connectionId);
                return Task.CompletedTask;
            }

            this.usernameConnectionMap[entries[0].Key] = null;

            return this.hubContext.Groups.RemoveFromGroupAsync(connectionId, this.password);
        }

        public ValueTask DisposeAsync() => this.DisposeAsyncCore();

        protected virtual async ValueTask DisposeAsyncCore()
        {
            foreach (var entry in this.usernameConnectionMap)
            {
                if (entry.Value is not null) await this.RemoveConnection(entry.Value);
            }
        }

        private IEnumerable<string> GetConnections(IEnumerable<string> usernames)
        {
            foreach (var username in usernames)
            {
                var connectionId = this.usernameConnectionMap[username];
                if (connectionId is not null)
                {
                    yield return connectionId;
                }
            }
        }

        private class NullGameClient : IGameClient {}
    }
}