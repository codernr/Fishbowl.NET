using System;
using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Services
{
    public interface IUserIdProvider
    {
        ValueTask<Guid> GetUserId();
    }

    public class UserIdProvider : IUserIdProvider
    {
        private const string UserIdKey = "user.id";

        private readonly IStorageService storageService;

        public UserIdProvider(IStorageService storageService) =>
            this.storageService = storageService;

        public async ValueTask<Guid> GetUserId()
        {
            Guid id;

            var storedId = await this.storageService.GetItem(UserIdKey);

            if (storedId is null)
            {
                id = Guid.NewGuid();
                await this.storageService.SetItem(UserIdKey, id.ToString());
                return id;
            }

            return new Guid(storedId);
        }
    }
}