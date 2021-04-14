using System;
using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Services
{
    public interface IUserIdProvider
    {
        Guid GetUserId();
    }

    public class UserIdProvider : IUserIdProvider
    {
        private const string UserIdKey = "user.id";

        private readonly IStorageService storageService;

        public UserIdProvider(IStorageService storageService) =>
            this.storageService = storageService;

        public Guid GetUserId()
        {
            Guid id;

            var storedId = this.storageService.GetItem(UserIdKey);

            if (storedId is null)
            {
                id = Guid.NewGuid();
                this.storageService.SetItem(UserIdKey, id.ToString());
                return id;
            }

            return new Guid(storedId);
        }
    }
}