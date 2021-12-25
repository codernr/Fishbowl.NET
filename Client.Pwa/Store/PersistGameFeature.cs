using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Pwa.Common;
using Fishbowl.Net.Client.Shared.Store;
using Fluxor;

namespace Fishbowl.Net.Client.Pwa.Store
{
    public class PersistGameEffects : IEffect
    {
        private readonly GameProperty persistedGame;

        private readonly IState<GamePlayState> state;

        private static readonly List<Type> Actions = new()
        {
            typeof(StartGameAction), typeof(SetPeriodSetupAction), typeof(SetPeriodPeriodAction),
            typeof(SetPeriodFinishedAction), typeof(SetPeriodPlayScoreCountAction)
        };

        public PersistGameEffects(GameProperty persistedGame, IState<GamePlayState> state) =>
            (this.persistedGame, this.state) = (persistedGame, state);

        public Task HandleAsync(object action, IDispatcher dispatcher)
        {
            this.persistedGame.Value = this.state.Value.Game?.Game;
            return Task.CompletedTask;
        }

        public bool ShouldReactToAction(object action){System.Console.WriteLine(action.GetType().ToString() + Actions.Contains(action.GetType())); return Actions.Contains(action.GetType()); }
    }
}