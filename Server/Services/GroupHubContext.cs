using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Server.Hubs;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Services
{
    public interface IGroupHubContext : IAsyncDisposable
    {
        int ConnectionCount { get; }

        bool ContainsKey(Guid id);

        IGameClient Group();

        IGameClient Client(Guid id);

        Task RegisterConnection(Guid id, string connectionId);

        Task RemoveConnection(string connectionId);
    }

    public class GroupHubContext : IGroupHubContext
    {
        public int ConnectionCount => this.idConnectionMap.Count;

        private static readonly IGameClient NullClient = new NullGameClient();

        private readonly Dictionary<Guid, string?> idConnectionMap = new();

        private readonly IHubContext<GameHub, IGameClient> hubContext;

        private readonly string password;

        public GroupHubContext(
            IHubContext<GameHub, IGameClient> hubContext,
            string password) =>
            (this.hubContext, this.password) = (hubContext, password);

        public bool ContainsKey(Guid id) => this.idConnectionMap.ContainsKey(id);

        public IGameClient Client(Guid playerId)
        {
            var connectionId = this.idConnectionMap[playerId];

            return (connectionId is null) ? NullClient : this.hubContext.Clients.Clients(connectionId);
        }

        public IGameClient Group() => this.hubContext.Clients.Group(this.password);

        public Task RegisterConnection(Guid playerId, string connectionId)
        {
            this.idConnectionMap[playerId] = connectionId;

            return this.hubContext.Groups.AddToGroupAsync(connectionId, this.password);
        }

        public Task RemoveConnection(string connectionId)
        {
            var key = this.idConnectionMap.First(entry => entry.Value == connectionId).Key;
            this.idConnectionMap[key] = null;

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

        private class NullGameClient : IGameClient
        {
            public Task ReceiveGameAborted(string message) => Task.CompletedTask;

            public Task ReceiveGameFinished(Game game) => Task.CompletedTask;

            public Task ReceiveGameStarted(Game game) => Task.CompletedTask;

            public Task ReceivePeriodFinished(Period period) => Task.CompletedTask;

            public Task ReceivePeriodSetup(Period period) => Task.CompletedTask;

            public Task ReceivePeriodStarted(Period period) => Task.CompletedTask;

            public Task ReceiveRoundFinished(Round round) => Task.CompletedTask;

            public Task ReceiveRoundStarted(Round round) => Task.CompletedTask;

            public Task ReceiveScoreAdded(Score score) => Task.CompletedTask;

            public Task ReceiveWordSetup(Word word) => Task.CompletedTask;
        }
    }
}