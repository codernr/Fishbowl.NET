using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Components.States
{
    public partial class PlayerWords
    {
        [Parameter]
        public EventCallback<string[]> OnPlayerWordsSet { get; set; } = default!;

        [Parameter]
        public int? WordCount { get; set; }

        private string[] words = default!;

        protected override void OnParametersSet()
        {
            this.words = new string[this.WordCount ?? 2];
            base.OnParametersSet();
        }
    }
}