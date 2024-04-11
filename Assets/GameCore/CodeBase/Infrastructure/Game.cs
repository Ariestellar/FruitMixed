using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.States;
using YG;

namespace CodeBase.Infrastructure
{
    public class Game
    {
        public GameStateMachine generalStateMachine;
        public Game(ICoroutineRunner coroutineRunner, YandexGame yandexGame, bool isDebug = false) 
            => generalStateMachine = new GameStateMachine(new SceneLoader(coroutineRunner), coroutineRunner, yandexGame, AllServices.Container, isDebug);
    }
}