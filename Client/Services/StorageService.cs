using System;
using System.Text.Json;
using Fishbowl.Net.Client.Shared;
using Microsoft.JSInterop;

namespace Fishbowl.Net.Client.Services
{
    public interface IStorageService
    {
        string? Password { get; set; }

        Guid? UserId { get; set; }

        ClientState ClientState { get; set; }
    }

    public class StorageService : IStorageService
    {
        private const string PasswordKey = "password";

        private const string UserIdKey = "user.id";

        private const string ClientStateKey = "client.state";

        private static readonly JsonSerializerOptions JsonSerializerOptions =
            new(JsonSerializerDefaults.Web);

        private readonly IJSInProcessRuntime js;

        public StorageService(IJSRuntime js) => this.js = (IJSInProcessRuntime)js;

        public ClientState ClientState
        {
            get
            {
                var json = this.GetItem(ClientStateKey);
                
                if (json is null)
                {
                    return new ClientState();
                }

                return JsonSerializer.Deserialize<ClientState>(json, JsonSerializerOptions) ?? new ClientState();
            }
            set => this.SetItem(ClientStateKey, JsonSerializer.Serialize(value, JsonSerializerOptions));
        }

        public string? Password
        {
            get => this.GetItem(PasswordKey);
            set => this.SetItem(PasswordKey, value);
        }

        public Guid? UserId
        {
            get
            {
                var id = this.GetItem(UserIdKey);

                return id is not null ? new Guid(id) : null;
            }
            set => this.SetItem(UserIdKey, value?.ToString());
        }

        private void SetItem(string key, string? value)
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

        private string? GetItem(string key) =>
            this.js.Invoke<string?>("StorageModule.getItem", key);
    }
}