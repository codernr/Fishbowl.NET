using System;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class PlayerData
    {
        [Parameter]
        public EventCallback<Player> OnPlayerDataSet { get; set; } = default!;

        private StateManager? innerStateManager;

        private StateManager InnerStateManager => this.innerStateManager ?? throw new InvalidOperationException();

        private string playerName = string.Empty;

        private Task SetPlayerNameAsync(string name)
        {
            this.playerName = name;
            return this.InnerStateManager.SetStateAsync<PlayerWords>();
        }

        private Task SetPlayerWordsAsync(string[] words) => this.SubmitAsync(this.playerName, words);

        private Task SubmitAsync(string name, string[] words) =>
            this.OnPlayerDataSet.InvokeAsync(new Player(
                Guid.NewGuid(),
                name,
                words.Select(word => new Word(Guid.NewGuid(), word))));
    }
}