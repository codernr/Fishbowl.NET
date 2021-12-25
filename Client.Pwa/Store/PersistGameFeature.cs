using System.Threading.Tasks;
using Fishbowl.Net.Client.Pwa.Common;
using Fishbowl.Net.Client.Shared.Store;
using Fluxor;

namespace Fishbowl.Net.Client.Pwa.Store
{
    public class PersistGameEffects
    {
        private readonly GameProperty persistedGame;

        private readonly IState<GamePlayState> state;

        public PersistGameEffects(GameProperty persistedGame, IState<GamePlayState> state) =>
            (this.persistedGame, this.state) = (persistedGame, state);

        [EffectMethod(typeof(StartStateManagerTransitionAction))]
        public Task OnStartStateManagerTransitionAction(IDispatcher dispatcher) => this.Persist();

        [EffectMethod(typeof(SetPeriodPlayScoreCountAction))]
        public Task OnSetPeriodPlayScoreCount(IDispatcher dispatcher) => this.Persist();

        private Task Persist()
        {
            this.persistedGame.Value = this.state.Value.Game?.Game;
            return Task.CompletedTask;
        }
    }
}