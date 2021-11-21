using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Fishbowl.Net.Client.Shared.Components
{
    public partial class Timer
    {
        [Parameter]
        public DateTimeOffset Start { get; set; }

        [Parameter]
        public TimeSpan Length { get; set; }

        [Parameter]
        public EventCallback OnExpired { get; set; }

        private string Classes => this.remaining > TimeSpan.FromSeconds(15) ?
            "mud-theme-info" : (this.remaining > TimeSpan.FromSeconds(5) ? "mud-theme-warning" : "mud-theme-error");

        private string TimeFormat =>
            string.Format("{0}{1:mm\\:ss}", this.Prefix, this.remaining);

        private string Prefix => this.remaining < TimeSpan.Zero ? "-" : string.Empty;

        private TimeSpan remaining;

        private Task? timerLoop;

        private CancellationTokenSource cancellationTokenSource = new();

        protected override Task OnInitializedAsync()
        {
            this.timerLoop = this.StartTimer(this.cancellationTokenSource.Token);
            return base.OnInitializedAsync();
        }

        private async Task StartTimer(CancellationToken cancellationToken)
        {
            this.UpdateRemaining();

            while (!cancellationToken.IsCancellationRequested)
            {
                var previous = this.remaining;
                await Task.Delay(500);
                this.UpdateRemaining();

                if (this.remaining < TimeSpan.Zero && previous > TimeSpan.Zero)
                {
                    await this.OnExpired.InvokeAsync();
                }
            }
        }

        private void UpdateRemaining()
        {
            this.remaining = this.Start + this.Length - DateTimeOffset.UtcNow;
            this.StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            if (this.timerLoop is null) return;

            this.cancellationTokenSource.Cancel();
            await this.timerLoop;
        }
    }
}