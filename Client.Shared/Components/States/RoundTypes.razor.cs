using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class RoundTypes
    {
        [Parameter]
        public EventCallback<string[]> OnRoundTypesSet { get; set; } = default!;

        private bool IsValid => this.options.Any(option => option.selected);

        private List<(string name, bool selected)> options = default!;

        private string[] SelectedOptions =>
            this.options
                .Where(option => option.selected)
                .Select(option => option.name)
                .ToArray();

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.options = new()
            {
                (L["Components.States.RoundTypes.Types.Taboo"]?.Value ?? "Taboo", true),
                (L["Components.States.RoundTypes.Types.Charades"]?.Value ?? "Charades", true),
                (L["Components.States.RoundTypes.Types.Password"]?.Value ?? "Password", true),
                (L["Components.States.RoundTypes.Types.Humming"]?.Value ?? "Humming", true)
            };
        }

        private void ToggleOption(int id)
        {
            this.options[id] = (this.options[id].name, !this.options[id].selected);
            this.Update();
        }
    }
}