using System;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.Views
{
    public partial class PlayerData
    {
        [Parameter]
        public EventCallback<Player> OnPlayerDataSet { get; set; } = default!;

        private string playerName = string.Empty;

        private string[] words = new string[2];

        private Task SubmitAsync(EventArgs e) =>
            this.OnPlayerDataSet.InvokeAsync(new Player(
                Guid.NewGuid(),
                this.playerName,
                this.words.Select(word => new Word(Guid.NewGuid(), word))));
    }
}