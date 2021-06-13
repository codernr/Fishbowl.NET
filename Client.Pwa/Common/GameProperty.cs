using System.Text.Json;
using Fishbowl.Net.Client.Shared.Common;
using Fishbowl.Net.Client.Shared.Services;
using Fishbowl.Net.Shared.GameEntities;

namespace Fishbowl.Net.Client.Pwa.Common
{
    public class GameProperty : PersistedProperty<Game?>
    {
        private readonly JsonSerializerOptions options;

        public GameProperty(IStorageService storageService, JsonSerializerOptions options) : base("game", null, storageService) =>
            this.options = options;

        protected override Game? Get()
        {
            var stringValue = this.storageService.GetItem(this.storageKey);

            return stringValue is null ? null : JsonSerializer.Deserialize<Game>(stringValue, this.options);
        }

        protected override void Set(Game? value) =>
            this.storageService.SetItem(this.storageKey, JsonSerializer.Serialize(value, this.options));
    }
}