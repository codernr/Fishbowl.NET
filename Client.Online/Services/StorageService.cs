using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Online.Services
{
    public interface IStorageService
    {
        string? GetItem(string key);

        void SetItem(string key, string? value);
    }

    public class StorageService : IStorageService
    {
        private readonly IJSInProcessRuntime js;

        public StorageService(IJSRuntime js) => this.js = (IJSInProcessRuntime)js;

        public void SetItem(string key, string? value)
        {
            if (value is null)
            {
                this.js.InvokeVoid("StorageModule.removeItem", key);
            }
            else
            {
                this.js.InvokeVoid("StorageModule.setItem", key, value);
            }
        }

        public string? GetItem(string key) =>
            this.js.Invoke<string?>("StorageModule.getItem", key);
    }
}