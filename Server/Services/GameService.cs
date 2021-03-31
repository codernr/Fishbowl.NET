using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Data;
using Fishbowl.Net.Shared.Reactive;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Services
{
    public class GameService
    {
        private TaskCompletionSource playersCreated = new();

        private Dictionary<string, Player?> players = new();

        private int teamCount = default!;

        private IEnumerable<string> roundTypes = default!;

        private GameManager gameManager = default!;

        public Observable<Round> Rounds { get; private set; } = default!;

        public async Task JoinGameAsync(string connectionId, IClientProxy caller)
        {
            if (players.ContainsKey(connectionId))
            {
                return;
            }

            players.Add(connectionId, null);

            if (players.Count == 1)
            {
                await caller.SendAsync("GetTeamCount");
            }
        }

        public Task SetPlayerAsync(string connectionId, string name, IEnumerable<Word> words)
        {
            this.players[connectionId] = new Player(Guid.NewGuid(), name, words);

            if (this.players.All(item => item.Value is not null))
            {
                this.CreateGameManager();
                this.playersCreated.SetResult();
            }

            return this.playersCreated.Task;
        }

        public Task SetTeamCountAsync(IClientProxy caller, int teamCount)
        {
            this.teamCount = teamCount;
            return caller.SendAsync("GetRoundTypes");
        }

        public void SetRoundTypes(IEnumerable<string> roundTypes) => this.roundTypes = roundTypes;

        private void CreateGameManager()
        {
            this.gameManager = new GameManager(
                Guid.NewGuid(), this.players.Values.Select(p => p!), this.roundTypes, this.teamCount);
        }
    }
}