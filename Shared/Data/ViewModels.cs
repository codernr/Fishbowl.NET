using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public record TeamViewModel(int Id, IEnumerable<Guid> PlayerIds); 
    
    public record TeamResultViewModel(int Id, int Score);
}