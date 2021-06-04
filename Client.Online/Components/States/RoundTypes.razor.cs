using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Online.Components.States
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
                (L["Components.States.RoundTypes.Types.Taboo"] ?? "Taboo", true),
                (L["Components.States.RoundTypes.Types.Charades"] ?? "Charades", true),
                (L["Components.States.RoundTypes.Types.Password"] ?? "Password", true),
                (L["Components.States.RoundTypes.Types.Humming"] ?? "Humming", true)
            };
        }

        private void ToggleOption(int id)
        {
            this.options[id] = (this.options[id].name, !this.options[id].selected);
            this.Update();
        }
    }
}