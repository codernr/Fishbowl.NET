using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Shared.Services
{
    public interface IStorageService
    {
        Task<string?> GetItem(string key);

        Task SetItem(string key, string? value);
    }

    public class StorageService : IStorageService
    {
        private const string JSModuleName = "StorageModule";

        private readonly Lazy<Task<IJSInProcessObjectReference>> moduleTask;

        public StorageService(IJSRuntime js) =>
            this.moduleTask = new (() => js.InvokeAsync<IJSInProcessObjectReference>(
                "import", "./_content/Fishbowl.Net.Client.Shared/js/storage.js").AsTask());

        public Task SetItem(string key, string? value) =>
            value is null ? this.InvokeVoid("removeItem", key) : this.InvokeVoid("setItem", key, value);

        public Task<string?> GetItem(string key) =>
            this.Invoke<string?>("getItem", key);

        private async Task InvokeVoid(string methodName, params object?[] args)
        {
            var module = await this.moduleTask.Value;
            module.InvokeVoid($"{JSModuleName}.{methodName}", args);
        }

        private async Task<T> Invoke<T>(string methodName, params object?[] args)
        {
            var module = await this.moduleTask.Value;
            return module.Invoke<T>($"{JSModuleName}.{methodName}", args);
        }
    }
}