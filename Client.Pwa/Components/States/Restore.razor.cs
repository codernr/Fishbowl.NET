using System;
using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Pwa.Components.States
{
    public partial class Restore
    {
        public Func<Task> OnRestoreRequested { get; set; } = default!;

        public Func<Task> OnNewGameRequested { get; set; } = default!;
    }
}