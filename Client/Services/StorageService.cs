using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Services
{
    public interface IStorageService
    {
        void SetItem(string key, string value);

        string? GetItem(string key);
    }

    public class StorageService : IStorageService
    {
        private readonly IJSInProcessRuntime js;

        public StorageService(IJSRuntime js) => this.js = (IJSInProcessRuntime)js;

        public void SetItem(string key, string value) =>
            this.js.InvokeVoid("StorageModule.setItem", key, value);

        public string? GetItem(string key) =>
            this.js.Invoke<string?>("StorageModule.getItem", key);
    }
}