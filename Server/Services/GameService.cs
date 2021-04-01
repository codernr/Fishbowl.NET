using System;
using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Server.Services
{
    public class GameService
    {
        private Dictionary<string, Player?> players = new();

        private int? teamCount;

        private IEnumerable<string>? roundTypes;

        private GameManager? gameManager;

        private IEnumerable<string> RoundTypes => this.roundTypes ??
            throw new InvalidOperationException("Invalid game state: RoundTypes are not defined");

        private int TeamCount => this.teamCount ??
            throw new InvalidOperationException("Invalid game state: TeamCount is not defined");

        public GameManager GameManager => this.gameManager ??
            throw new InvalidOperationException("Invalid game state: GameManager is not defined");

        public bool Connect(string connectionId)
        {
            bool askForTeamCount = players.Count == 0;

            if (!this.players.ContainsKey(connectionId))
            {
                this.players.Add(connectionId, null);
            }

            return askForTeamCount;
        }

        public void SetTeamCount(int teamCount)
        {
            this.teamCount = teamCount;
        }

        public void SetRoundTypes(IEnumerable<string> roundTypes) => this.roundTypes = roundTypes;

        public bool SetPlayer(string connectionId, Player player)
        {
            this.players[connectionId] = player;

            if (this.players.All(item => item.Value is not null))
            {
                this.CreateGameManager();
                return true;
            }

            return false;
        }

        private void CreateGameManager()
        {
            this.gameManager = new GameManager(
                Guid.NewGuid(),
                this.players.Keys.Select(id => this.players[id] ??
                    throw new InvalidOperationException($"No player defined for connection: {id}")),
                this.RoundTypes, this.TeamCount);
        }
    }
}