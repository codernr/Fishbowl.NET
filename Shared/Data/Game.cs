using System;
using System.Collections.Generic;
using System.Linq;

namespace Fishbowl.Net.Shared.Data
{
    public class Game
    { 
        private readonly CircularEnumerator<Team> teamEnumerator;

        private readonly IEnumerator<Round> roundEnumerator;

        private TimeSpan? remaining;

        public Guid Id { get; private set; }

        public IList<Team> Teams { get; private set; }
        
        public IList<Round> Rounds { get; private set; }
        
        public TimeSpan PeriodLength { get; private set; } = TimeSpan.FromSeconds(60);

        public TimeSpan PeriodThreshold { get; private set; } = TimeSpan.FromSeconds(5);

        public Game(
            Guid id,
            IEnumerable<Player> players,
            IEnumerable<string> roundTypes,
            int teamCount = 2,
            bool randomize = true)
        {
            this.Id = id;

            var playerList = (randomize ? players.Randomize() : players).ToList();

            if (playerList.Count < teamCount * 2)
            {
                throw new ArgumentException("Player count should be at least (team count) * 2.");
            }

            this.Teams = players
                .Distribute(teamCount)
                .Select((players, id) => new Team(id, players.ToList()))
                .ToList();

            this.teamEnumerator = new CircularEnumerator<Team>(this.Teams);

            var words = players
                .SelectMany(player => player.Words);

            this.Rounds = roundTypes
                .Select(type => new Round(
                    type,
                    randomize ? new RandomEnumerator<Word>(words) : new RewindEnumerator<Word>(words)))
                .ToList();

            this.roundEnumerator = this.Rounds.GetEnumerator();
        }

        public IEnumerable<Round> RoundLoop()
        {
            while(this.roundEnumerator.MoveNext())
            {
                yield return this.roundEnumerator.Current;
            }
        }

        public IEnumerable<Period> PeriodLoop()
        {
            while (this.roundEnumerator.Current.NextPeriod(
                this.remaining ?? this.PeriodLength,
                this.teamEnumerator.Current.PlayerEnumerator().Current))
            {
                yield return this.roundEnumerator.Current.CurrentPeriod();
            }
        }

        public Word CurrentWord() => this.roundEnumerator.Current.WordEnumerator().Current;

        public void StartPeriod(DateTimeOffset timestamp) =>
            this.roundEnumerator.Current.CurrentPeriod().StartedAt = timestamp;

        public void AddScore(Score score) => this.roundEnumerator.Current.CurrentPeriod().Scores.Add(score);

        public bool NextWord(DateTimeOffset timestamp)
        {
            var period = this.roundEnumerator.Current.CurrentPeriod();

            if (timestamp >= period.StartedAt + period.Length - this.PeriodThreshold)
            {
                this.FinishPeriod(timestamp, false);
                return false;
            }

            if (!this.roundEnumerator.Current.WordEnumerator().MoveNext())
            {
                period.FinishedAt = timestamp;
                this.remaining = period.StartedAt + period.Length - timestamp;
                return false;
            }

            return true;
        }

        public Dictionary<int, int> GetTeamScores()
        {
            var playerScores = this.Rounds
                .SelectMany(round => round.Periods)
                .GroupBy(period => period.Player.Id)
                .ToDictionary(item => item.Key, item => item.SelectMany(p => p.Scores).Count());

            var teamScores = this.Teams
                .ToDictionary(
                    team => team.Id,
                    team => playerScores
                        .Where(score => team.Players
                            .Any(player => player.Id == score.Key))
                        .Select(item => item.Value)
                        .Sum());

            return teamScores;
        }

        public void FinishPeriod(DateTimeOffset timestamp) =>
            this.FinishPeriod(timestamp, true);

        private void FinishPeriod(DateTimeOffset timestamp, bool rewindWord)
        {
            this.roundEnumerator.Current.CurrentPeriod().FinishedAt = timestamp;
            this.remaining = null;
            this.teamEnumerator.Current.PlayerEnumerator().MoveNext();
            this.teamEnumerator.MoveNext();

            if (rewindWord) this.roundEnumerator.Current.WordEnumerator().MovePrevious();
        }
    }
}