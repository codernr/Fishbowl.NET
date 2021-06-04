using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Shared.Services
{
    public interface IStorageService
    {
        Task InitializeAsync();
        
        string? GetItem(string key);

        void SetItem(string key, string? value);
    }

    public class StorageService : IStorageService
    {
        private const string JSModuleName = "StorageModule";

        private readonly IJSRuntime js;

        private IJSInProcessObjectReference? module;

        private IJSInProcessObjectReference Module => this.module ?? throw new InvalidOperationException();

        public StorageService(IJSRuntime js) => this.js = js;

        public async Task InitializeAsync() => this.module = await this.js.InvokeAsync<IJSInProcessObjectReference>(
                "import", "./_content/Fishbowl.Net.Client.Shared/js/storage.js");

        public void SetItem(string key, string? value)
        {
            if (value is null) this.InvokeVoid("removeItem", key);
            else this.InvokeVoid("setItem", key, value);
        }

        public string? GetItem(string key) => this.Invoke<string?>("getItem", key);

        private void InvokeVoid(string methodName, params object?[] args) =>
            this.Module.InvokeVoid($"{JSModuleName}.{methodName}", args);

        private T Invoke<T>(string methodName, params object?[] args) =>
            this.Module.Invoke<T>($"{JSModuleName}.{methodName}", args);
    }
}