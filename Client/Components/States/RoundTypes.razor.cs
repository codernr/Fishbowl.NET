using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class RoundTypes
    {
        [Parameter]
        public EventCallback<string[]> OnRoundTypesSet { get; set; } = default!;

        private bool IsValid => this.options.Any(option => option.selected);

        private List<(string name, bool selected)> options = new()
        {
            ("Taboo", true),
            ("Charades", true),
            ("Password", true),
            ("Humming", true)
        };

        private string[] SelectedOptions =>
            this.options
                .Where(option => option.selected)
                .Select(option => option.name)
                .ToArray();

        private void ToggleOption(int id)
        {
            this.options[id] = (this.options[id].name, !this.options[id].selected);
            this.Update();
        }
    }
}