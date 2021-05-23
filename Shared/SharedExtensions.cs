using System;
using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Shared.GameEntities;

namespace Fishbowl.Net.Shared
{
    public static class SharedExtensions
    {
        private static readonly Random random = new();

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source) =>
            source.OrderBy<T, int>((item) => random.Next());

        public static IEnumerable<IEnumerable<T>> Distribute<T>(this IEnumerable<T> source, int groupCount) =>
            source
                .Select((item, i) => (id: i, element: item))
                .GroupBy(item => item.id % groupCount, item => item.element);

        public static IEnumerable<Team> CreateTeams(this IEnumerable<Player> players, int teamCount) =>
            players
                .Distribute(teamCount)
                .Select((teamPlayers, id) => new Team(id, teamPlayers.ToList()))
                .ToList();
    }
}