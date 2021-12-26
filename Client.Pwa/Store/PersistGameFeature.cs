using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Pwa.Common;
using Fishbowl.Net.Shared.Actions;
using Fluxor;

namespace Fishbowl.Net.Client.Pwa.Store
{
    public class PersistGameEffects : IEffect
    {
        private readonly GameProperty persistedGame;

        private readonly IState<GamePlayState> state;

        private static readonly List<Type> Actions = new()
        {
            typeof(StartGameAction), typeof(ReceivePeriodSetupAction), typeof(ReceivePeriodStartedAction),
            typeof(ReceivePeriodFinishedAction), typeof(ReceiveScoreAddedAction)
        };

        public PersistGameEffects(GameProperty persistedGame, IState<GamePlayState> state) =>
            (this.persistedGame, this.state) = (persistedGame, state);

        public Task HandleAsync(object action, IDispatcher dispatcher)
        {
            this.persistedGame.Value = this.state.Value.Game?.Game;
            return Task.CompletedTask;
        }

        public bool ShouldReactToAction(object action) =>
            Actions.Contains(action.GetType());
    }
}