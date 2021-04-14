using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Services
{
    public interface IStorageService
    {
        ValueTask SetItem(string key, string value);

        ValueTask<string?> GetItem(string key);
    }

    public class StorageService : IStorageService
    {
        private readonly IJSRuntime js;

        private IJSObjectReference? module;

        public StorageService(IJSRuntime js) => this.js = js;

        public async ValueTask SetItem(string key, string value)
        {
            var module = await this.GetModule();

            await module.InvokeVoidAsync("storageModule.setItem", key, value);
        }

        public async ValueTask<string?> GetItem(string key)
        {
            var module = await this.GetModule();

            return await module.InvokeAsync<string?>("storageModule.getItem", key);
        }

        private async Task<IJSObjectReference> GetModule()
        {
            if (this.module is null)
            {
                this.module = await this.js.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/js/storageModule.js");
            }

            return this.module;
        }
    }
}