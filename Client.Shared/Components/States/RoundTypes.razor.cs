using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class RoundTypes
    {
        [Parameter]
        public EventCallback<string[]> OnRoundTypesSet { get; set; } = default!;

        private bool IsValid => this.selected.Count() > 0;

        private string[] options = default!;

        private IEnumerable<string> selected = default!;

        private string[] SelectedOptions => this.selected.ToArray();

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.options = new[]
            {
                L["Components.States.RoundTypes.Types.Taboo"]?.Value ?? "Taboo",
                L["Components.States.RoundTypes.Types.Drawing"]?.Value ?? "Drawing",
                L["Components.States.RoundTypes.Types.Charades"]?.Value ?? "Charades",
                L["Components.States.RoundTypes.Types.Password"]?.Value ?? "Password",
                L["Components.States.RoundTypes.Types.Humming"]?.Value ?? "Humming",
            };

            this.selected = new HashSet<string>(this.options);
        }
    }
}