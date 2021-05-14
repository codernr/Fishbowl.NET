using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Server.Services
{
    public interface IGroupHubContext : IAsyncDisposable
    {
        int Count { get; }

        bool ContainsKey(Guid id);

        IGameClient Group();

        IGameClient GroupExcept(IEnumerable<Guid> ids);

        IGameClient Client(Guid id);

        IGameClient Clients(IEnumerable<Guid> ids);

        Task RegisterConnection(Guid id, string connectionId);

        Task RemoveConnection(string connectionId);
    }

    public class GroupHubContext : IGroupHubContext
    {
        public int Count => this.idConnectionMap.Count;

        private static readonly IGameClient NullClient = new NullGameClient();

        private readonly Dictionary<Guid, string?> idConnectionMap = new();

        private readonly IHubContext<GameHub, IGameClient> hubContext;

        private readonly string password;

        private readonly ILogger<GroupHubContext> logger;

        public GroupHubContext(
            IHubContext<GameHub, IGameClient> hubContext,
            string password,
            ILogger<GroupHubContext> logger) =>
            (this.hubContext, this.password, this.logger) =
            (hubContext, password, logger);

        public bool ContainsKey(Guid id) => this.idConnectionMap.ContainsKey(id);

        public IGameClient Client(Guid playerId)
        {
            var connectionId = this.idConnectionMap[playerId];

            return (connectionId is null) ? NullClient : this.hubContext.Clients.Clients(connectionId);
        }

        public IGameClient Clients(IEnumerable<Guid> playerIds) =>
            this.hubContext.Clients.Clients(this.GetConnections(playerIds));

        public IGameClient Group() => this.hubContext.Clients.Group(this.password);

        public IGameClient GroupExcept(IEnumerable<Guid> playerIds) =>
            this.hubContext.Clients.GroupExcept(this.password, this.GetConnections(playerIds));

        public Task RegisterConnection(Guid playerId, string connectionId)
        {
            this.idConnectionMap[playerId] = connectionId;

            return this.hubContext.Groups.AddToGroupAsync(connectionId, this.password);
        }

        public Task RemoveConnection(string connectionId)
        {
            var entries = this.idConnectionMap.Where(entry => entry.Value == connectionId).ToList();

            if (entries.Count < 1)
            {
                this.logger.LogInformation("RemoveConnection: {ConnectionId} not found", connectionId);
                return Task.CompletedTask;
            }

            this.idConnectionMap[entries[0].Key] = null;

            return this.hubContext.Groups.RemoveFromGroupAsync(connectionId, this.password);
        }

        public ValueTask DisposeAsync() => this.DisposeAsyncCore();

        protected virtual async ValueTask DisposeAsyncCore()
        {
            foreach (var entry in this.idConnectionMap)
            {
                if (entry.Value is not null) await this.RemoveConnection(entry.Value);
            }
        }

        private IEnumerable<string> GetConnections(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                var connectionId = this.idConnectionMap[id];
                if (connectionId is not null)
                {
                    yield return connectionId;
                }
            }
        }

        private class NullGameClient : IGameClient {}
    }
}