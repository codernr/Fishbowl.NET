using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data.ViewModels
{
    public record PlayerCountViewModel(int SetupCount, int TotalCount);

    public record PlayerViewModel(Guid Id, string Name);

    public record TeamViewModel(int Id, IEnumerable<PlayerViewModel> Players);
}