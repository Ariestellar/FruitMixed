using System;
using System.Numerics;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using YG;

namespace CodeBase.Infrastructure.States
{
    public class LoadGameSceneState : IPayloadedState<Scenes>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly AllServices _services;
        private SceneLoader _sceneLoader;
        private SessionData _sessionData;
        private ViewProvider _viewProvider;
        private SoundService _soundService;
        private GameObjectFactory _gameObjectFactory;

        public LoadGameSceneState(GameStateMachine stateMachine, AllServices allServices, SceneLoader sceneLoader) {
            _stateMachine = stateMachine; 
            _services = allServices;
            _sceneLoader = sceneLoader;
        }

        public void Enter(Scenes nameScene) { 
            _sessionData = _services.Single<SessionData>();
            _viewProvider = _services.Single<ViewProvider>();
            _soundService = _services.Single<SoundService>();
            _gameObjectFactory = _services.Single<GameObjectFactory>();
            _sessionData.topScore = YandexGame.savesData.topScore;
            _sessionData.currentScore = YandexGame.savesData.currentScore;
            _sessionData.numberOfBomb = YandexGame.savesData.isFirstSession?2:YandexGame.savesData.numberOfBomb;
            _sessionData.numberOfBlend = YandexGame.savesData.isFirstSession?1:YandexGame.savesData.numberOfBlend;
            _sessionData.numberOfMultifruit = YandexGame.savesData.isFirstSession?3:YandexGame.savesData.numberOfMultifruit;
            _sessionData.numberOfCrystal = YandexGame.savesData.isFirstSession?0:YandexGame.savesData.numberOfCrystal;
            _sceneLoader.Load(nameScene.ToString(), OnLoad);
        }

        private async void OnLoad() {
            _viewProvider.gameSceneView = UnityEngine.GameObject.FindObjectOfType<GameSceneView>();
            _soundService.SetSoundSceneComponents(_viewProvider.gameSceneView.SoundSceneComponents, _sessionData);
            _viewProvider.gameSceneView.TopScore.text = Convert.ToString(_sessionData.topScore);
            _viewProvider.gameSceneView.CurrentScore.text = Convert.ToString(_sessionData.currentScore);

            _sessionData.fruitsInGlasseParent = _viewProvider.gameSceneView.FruitsInGlasseParent;
            if(YandexGame.savesData.typeFruit != null) await LoadFruits();
            StartGame();
        }
        private async Task LoadFruits() { 
            for (int i = 0; i < YandexGame.savesData.typeFruit.Length; i++) {
                UnityEngine.Vector3 positionSpawn = new UnityEngine.Vector3(YandexGame.savesData.positionFruitX[i], YandexGame.savesData.positionFruitY[i], YandexGame.savesData.positionFruitZ[i]);
                UnityEngine.Quaternion rotationSpawn = UnityEngine.Quaternion.Euler(YandexGame.savesData.rotationFruitX[i], YandexGame.savesData.rotationFruitY[i], YandexGame.savesData.rotationFruitZ[i]);
                Fruit fruit = await _gameObjectFactory.CreateFruit((FruitType)YandexGame.savesData.typeFruit[i], positionSpawn, rotationSpawn, FruitState.Collision);
                fruit.transform.parent = _sessionData.fruitsInGlasseParent;
            }           
        }
        private void StartGame() => _stateMachine.Enter<GameRunnerState>();

        public void Exit() {
            
        }
    }
}