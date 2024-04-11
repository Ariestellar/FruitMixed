using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;

namespace CodeBase.Infrastructure.States
{
    public class BootstrapState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly AllServices _allServices;
        private readonly ICoroutineRunner _coroutineRunner;

        public BootstrapState(GameStateMachine stateMachine, ICoroutineRunner coroutineRunner, AllServices allServices) {
            _stateMachine = stateMachine;
            _allServices = allServices;
            _coroutineRunner = coroutineRunner;
            RegisterServices();
        }

        public void Enter() { 
            _stateMachine.Enter<LoadGameSceneState, Scenes>(Scenes.GameScene);
        }

        public void Exit() { }

        private void RegisterServices() { 
			AssetProvider assetProvider = new AssetProvider();
			InputService inputService = new InputService();
            inputService.Enable();

			_allServices.RegisterSingle<AssetProvider>(assetProvider);
			_allServices.RegisterSingle<UIFactory>(new UIFactory(assetProvider));
			_allServices.RegisterSingle<GameObjectFactory>(new GameObjectFactory(assetProvider));
			_allServices.RegisterSingle<EffectsFactory>(new EffectsFactory(assetProvider, _coroutineRunner));
			_allServices.RegisterSingle<SessionData>(new SessionData()); 
			_allServices.RegisterSingle<ViewProvider>(new ViewProvider()); 
			_allServices.RegisterSingle<InputService>(inputService);
			_allServices.RegisterSingle<SoundService>(new SoundService());
		}        
    }
}

