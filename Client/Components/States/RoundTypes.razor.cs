using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class RoundTypes
    {
        [Parameter]
        public EventCallback<IEnumerable<string>> OnRoundTypesSet { get; set; } = default!;

        private List<(string name, bool selected)> options = new()
        {
            ("Taboo", true),
            ("Charades", true),
            ("Password", true),
            ("Humming", true)
        };

        private IEnumerable<string> SelectedOptions =>
            this.options
                .Where(option => option.selected)
                .Select(option => option.name);

        private void ToggleOption(int id) =>
            this.options[id] = (this.options[id].name, !this.options[id].selected);
    }
}