using System;

namespace Fishbowl.Net.Client.Shared.Services
{
    public abstract class PersistedProperty<T>
    {
        protected readonly string storageKey;

        protected readonly T defaultValue;

        protected readonly IStorageService storageService;

        protected bool isInitialized = false;

        protected T value = default!;

        public PersistedProperty(
            string storageKey,
            T defaultValue,
            IStorageService storageService) =>
            (this.storageKey, this.defaultValue, this.storageService) =
            (storageKey, defaultValue, storageService);

        public T Value
        {
            get
            {
                if (!this.isInitialized)
                {
                    this.value = this.Get();
                    this.isInitialized = true;
                }

                return this.value;
            }
            set
            {
                this.value = value;
                this.Set(value);
            }
        }

        protected abstract T Get();

        protected virtual void Set(T value) => this.storageService.SetItem(this.storageKey, value?.ToString());
    }

    public class IdProperty : PersistedProperty<Guid>
    {
        public IdProperty(IStorageService storageService) : base("id", Guid.NewGuid(), storageService) {}

        protected override Guid Get()
        {
            var stringValue = this.storageService.GetItem(this.storageKey);
            if (stringValue is null)
            {
                this.storageService.SetItem(this.storageKey, this.defaultValue.ToString());
                return this.defaultValue;
            }
            
            return new Guid(stringValue);
        }
    }

    public class NameProperty : PersistedProperty<string>
    {
        public NameProperty(IStorageService storageService) : base("name", string.Empty, storageService) {}

        protected override string Get() => this.storageService.GetItem(this.storageKey) ?? this.defaultValue;
    }

    public class PasswordProperty : PersistedProperty<string?>
    {
        public PasswordProperty(IStorageService storageService) : base("password", null, storageService) {}

        protected override string? Get() => this.storageService.GetItem(this.storageKey);
    }

    public class ExpiresProperty : PersistedProperty<DateTime>
    {
        public static readonly TimeSpan DefaultExpiryTime = TimeSpan.FromMinutes(30);

        public ExpiresProperty(IStorageService storageService) :
            base("expires", DateTime.Now + DefaultExpiryTime, storageService) {}

        protected override DateTime Get()
        {
            var stringValue = this.storageService.GetItem(this.storageKey);
            return stringValue is null ? this.defaultValue : DateTime.Parse(stringValue);
        }
    }
}