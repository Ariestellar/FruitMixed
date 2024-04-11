using System.Collections;
using System.Collections.Generic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CodeBase.Infrastructure.States
{
    public class GameRunnerState : IState {
        private readonly GameStateMachine _stateMachine;
        private readonly AllServices _services;
        private readonly ICoroutineRunner _coroutineRunner;
        private InputService _inputService;
        private AssetProvider _assetProvider;
        private ViewProvider _viewProvider;
        private GameObject _spawnFruitPosition;
        private Transform _fruitsInGlasseParent;
        private EndLineGame _endLineGame;
        private Dictionary<ObjectInHand, IHelpmode> _helpModes = new Dictionary<ObjectInHand, IHelpmode>();
        private BombMode _bombMode;
        private BlenderMode _blenderMode;
        private MultifruitMode _multifruitMode;
        private SessionData _sessionData;
        private SoundService _soundService;
        private GameObjectFactory _gameObjectFactory;
        private EffectsFactory _effectsFactory;
        private UIFactory _uiFactory;
        private ScoreCounter _scoreCounter;
        private Fruit _currentFruitForDropp;
        private GameObject _helpPanel;
        private Coroutine _delayGetFruit;
        private ADDialog _adDialog;
        private ObjectInHand _currentObjectInHand;
        private UIMenu _currentUIMenu;

        public GameRunnerState(GameStateMachine stateMachine, ICoroutineRunner coroutineRunner, AllServices allServices) {
            _stateMachine = stateMachine; 
            _services = allServices;
            _coroutineRunner = coroutineRunner;
        }

        public void Enter() {
            _inputService = _services.Single<InputService>();
            _assetProvider = _services.Single<AssetProvider>();
            _viewProvider = _services.Single<ViewProvider>(); 
            _sessionData = _services.Single<SessionData>();
            _soundService = _services.Single<SoundService>();
            _gameObjectFactory = _services.Single<GameObjectFactory>();
            _effectsFactory = _services.Single<EffectsFactory>();
            _uiFactory = _services.Single<UIFactory>();

            _sessionData.currentGameState = StateGame.Game;

            _inputService.AddInactiveTouchZone(_viewProvider.gameSceneView.SoundSceneComponents.MuteSoundToggle.GetComponent<InactiveTouchZone>());
            _inputService.AddInactiveTouchZone(_viewProvider.gameSceneView.HelpToggle.GetComponent<InactiveTouchZone>()); 

            _viewProvider.gameSceneView.CrystalScore.text = _sessionData.numberOfCrystal.ToString();

            _spawnFruitPosition = _viewProvider.gameSceneView.SpawnFruitPosition;
            _fruitsInGlasseParent = _viewProvider.gameSceneView.FruitsInGlasseParent;
            _endLineGame = _viewProvider.gameSceneView.EndLineGame;

            _adDialog = new ADDialog(_uiFactory, _inputService, _assetProvider, _sessionData, _soundService);
            _scoreCounter = new ScoreCounter(_coroutineRunner, _viewProvider.gameSceneView.CurrentScore, _viewProvider.gameSceneView.TopScore, _sessionData);
            
            _bombMode = new BombMode(_adDialog, _viewProvider.gameSceneView.ToggleBombMode, _inputService, _gameObjectFactory, _effectsFactory, _assetProvider, _spawnFruitPosition.transform.position, _sessionData);
            _blenderMode = new BlenderMode(_coroutineRunner, _adDialog, _viewProvider.gameSceneView.ToggleBlenderMode, _inputService, _effectsFactory, _assetProvider, _sessionData);
            _multifruitMode = new MultifruitMode(_adDialog, _viewProvider.gameSceneView.ToggleMultifruitMode, _inputService, _gameObjectFactory, _assetProvider, _spawnFruitPosition.transform.position, _sessionData);
            _helpModes = new Dictionary<ObjectInHand, IHelpmode>(){{ObjectInHand.Bomb, _bombMode},{ObjectInHand.Blender, _blenderMode},{ObjectInHand.Multifruit, _multifruitMode}};            
            foreach (var item in _helpModes) item.Value.ModeSwitched.AddListener(SwitchHelpMode);
            _bombMode.FruitDestroyed.AddListener(FruitDestroy);
            _blenderMode.FruitDestroyed.AddListener(FruitDestroy);

            _endLineGame.TimeOnLineIsOvered.AddListener(FinishGame);
            _adDialog.SwitchADDialogPanel.AddListener(SwitchADPanel);
            _adDialog.CrystalsSpending.AddListener(CrystalSpent);

            _viewProvider.gameSceneView.HelpToggle.onValueChanged.AddListener(SwitchHelpPanel);

            _scoreCounter.MultiplierWorked.AddListener(SetMultiplierEffectAsync);

            Fruit[] fruitsInGlasse = _viewProvider.gameSceneView.FruitsInGlasseParent.GetComponentsInChildren<Fruit>();
            if(fruitsInGlasse != null) {
                for (int i = 0; i < fruitsInGlasse.Length; i++) {
                    fruitsInGlasse[i].CollisionWithAnotherFruitOccurred.AddListener(CombineFruits);
                }
            } 
            
            StartLevel();

            if(_stateMachine.IsDebug) {
                TestScripts testScripts = UnityEngine.GameObject.FindObjectOfType<TestScripts>(true);
                testScripts.gameObject.SetActive(true);
                _inputService.AddInactiveTouchZone(testScripts.SpawnButton.GetComponent<InactiveTouchZone>());
                _inputService.AddInactiveTouchZone(testScripts.InputField.GetComponent<InactiveTouchZone>());
                testScripts.Spawned.AddListener(SpawnForTestAsync);
            } 
        }

        private void CrystalSpent() => _viewProvider.gameSceneView.CrystalScore.text = _sessionData.numberOfCrystal.ToString();

        private async void SpawnForTestAsync(int value)
        {            
            Fruit Fruit = await _gameObjectFactory.CreateFruit((FruitType)value, _spawnFruitPosition.transform.position, Quaternion.identity, FruitState.Collision);
            Fruit.CollisionWithAnotherFruitOccurred.AddListener(CombineFruits);
        }

        public void Exit() {
            _sessionData.currentScore = _scoreCounter.TotalScore;
            OffControlDroppFruit(_currentFruitForDropp);
        } 

        private void StartLevel() { 
            _soundService.PlaySoundBackground();
            _scoreCounter.SetScore(_sessionData.currentScore);
            GetFruitForDropp();
        }
        
        private async void GetFruitForDropp() { 
            _currentFruitForDropp = await _gameObjectFactory.CreateFruit(GetTypeFruit(), _spawnFruitPosition.transform.position, Quaternion.identity, FruitState.Ready);
            if(_currentUIMenu == UIMenu.None) OnControlDroppFruit(_currentFruitForDropp);
            _currentObjectInHand = ObjectInHand.Fruit;
        }

        private FruitType GetTypeFruit() {
            int numberFruit = UnityEngine.Random.Range(0, 3);
            Fruit[] fruitsInGlasse = _viewProvider.gameSceneView.FruitsInGlasseParent.GetComponentsInChildren<Fruit>();
            if(fruitsInGlasse.Length >= 5) { 
                for (int i = 0; i < fruitsInGlasse.Length; i++) {
                    if(fruitsInGlasse[i].FruitType >= FruitType.Tangerine){
                        numberFruit = UnityEngine.Random.Range(0, 5);
                        break;
                    }
                }
            }else{
                numberFruit = UnityEngine.Random.Range(0, 3);
            }
            
            return (FruitType)numberFruit;
        }

        private void DroppFruit(Fruit fruit) {
            _currentObjectInHand = ObjectInHand.None;
            fruit.transform.parent = _fruitsInGlasseParent;
            _inputService.ScreenTouchPointPositionChanged.RemoveListener(fruit.Move);
            _inputService.TouchPointUped.RemoveListener(fruit.Release);
            GetNewFruitWithDelay(); 
        }

        private void GetNewFruitWithDelay() => _delayGetFruit = _coroutineRunner.StartCoroutine(DelayForAction(1, GetFruitForDropp));

        private async void CombineFruits(Fruit fruit1, Fruit fruit2) { 
            Fruit fruit = await _gameObjectFactory.CreateFruit(fruit1.FruitType + 1, (fruit1.transform.position + fruit2.transform.position) / 2, Quaternion.identity, FruitState.Collision); 

            if(fruit.FruitType == FruitType.Watermelon){
                ExplodeWatermelon(fruit);
            }else fruit.CollisionWithAnotherFruitOccurred.AddListener(CombineFruits);
            
            _scoreCounter.AddScore(fruit1.FruitScore, true);
            _soundService.PlaySoundMerge();
            fruit.transform.parent = _fruitsInGlasseParent;
            if(fruit2 != null) _assetProvider.Unload(fruit2.gameObject);
            if(fruit1 != null) _assetProvider.Unload(fruit1.gameObject);
        }

        private void FinishGame() => _stateMachine.Enter<FinishState>();

        private async void SwitchHelpPanel(bool value) {
            _currentUIMenu = UIMenu.Help;
            SwitchControlDroppForAllObject(value,_currentObjectInHand);
            if (value){ 
                _helpPanel = await _uiFactory.CreateHelpPanel();
                _helpPanel.GetComponentInChildren<Button>().onClick.AddListener(TurnOffHelpPanel);
            } else if(value == false && _helpPanel != null) {
                _assetProvider.Unload(_helpPanel);
                _currentUIMenu = UIMenu.None;
            }
        }

        private void SwitchADPanel(bool value) {
            _currentUIMenu = value?UIMenu.AD:UIMenu.None;
            SwitchControlDroppForAllObject(value,_currentObjectInHand);
        }

        private void SwitchControlDroppForAllObject(bool value, ObjectInHand objectInHand) {            
            switch (objectInHand) {
                case ObjectInHand.Bomb:
                case ObjectInHand.Blender:
                case ObjectInHand.Multifruit:
                    _helpModes[objectInHand].SwitchControlDropp(value);
                    break;
                case ObjectInHand.Fruit:
                    SwitchControlDroppFruit(value);
                    break;
                case ObjectInHand.None:
                    break;
                default:
                    Debug.LogError("Нет такого типа в ObjectInHand");
                    break;
            }
        }

        private void SwitchControlDroppFruit(bool value) {
            if(value){ 
                if(_currentFruitForDropp.FruitState == FruitState.Ready) OffControlDroppFruit(_currentFruitForDropp);
                else if(_currentFruitForDropp.FruitState != FruitState.Ready && _delayGetFruit != null)_coroutineRunner.StopCoroutine(_delayGetFruit);                
            } else {
                if(_currentFruitForDropp.FruitState == FruitState.Ready) OnControlDroppFruit(_currentFruitForDropp);
                else GetFruitForDropp();
            }
        }

        private void SwitchHelpMode(bool value, IHelpmode helpmode) {
            Debug.Log("SwitchHelpMode " + value + helpmode.ObjectInHand.ToString());
            if(value){ 
                _currentObjectInHand = helpmode.ObjectInHand;
                if(_currentFruitForDropp.FruitState == FruitState.Ready) OffCurrentFruit(_currentFruitForDropp);
                else if(_currentFruitForDropp.FruitState != FruitState.Ready && _delayGetFruit != null) _coroutineRunner.StopCoroutine(_delayGetFruit);
            } else { 
                _currentObjectInHand = ObjectInHand.None;
                if(_currentFruitForDropp.FruitState == FruitState.Ready ) {
                    if(helpmode.IsRelease) _coroutineRunner.StartCoroutine(DelayForAction(0.5f, () => OnCurrentFruit(_currentFruitForDropp)));
                    else OnCurrentFruit(_currentFruitForDropp);
                } else GetNewFruitWithDelay();
            }
        } 

        private void OnCurrentFruit(Fruit currentFruit) {
            _currentObjectInHand = ObjectInHand.Fruit;
            currentFruit.gameObject.SetActive(true);
            OnControlDroppFruit(currentFruit);
        }

        private void OffCurrentFruit(Fruit currentFruit) {
            currentFruit.gameObject.SetActive(false);
            OffControlDroppFruit(currentFruit);
        }
        
        private void TurnOffHelpPanel() => _viewProvider.gameSceneView.HelpToggle.isOn = false;

        private void OnControlDroppFruit(Fruit currentFruitForDropp) {
            currentFruitForDropp.Released.AddListener(DroppFruit);
            currentFruitForDropp.CollisionWithAnotherFruitOccurred.AddListener(CombineFruits);
            _inputService.ScreenTouchPointPositionChanged.AddListener(currentFruitForDropp.Move);
            _inputService.TouchPointUped.AddListener(currentFruitForDropp.Release);
        }

        private void OffControlDroppFruit(Fruit currentFruitForDropp) {
            currentFruitForDropp.Released.RemoveListener(DroppFruit);
            currentFruitForDropp.CollisionWithAnotherFruitOccurred.RemoveListener(CombineFruits);
            _inputService.ScreenTouchPointPositionChanged.RemoveListener(currentFruitForDropp.Move);
            _inputService.TouchPointUped.RemoveListener(currentFruitForDropp.Release);
        }

        private async void SetMultiplierEffectAsync(int valueMultiplier) 
            => await _effectsFactory.CreateMultiplierEffect(_viewProvider.gameSceneView.SpawnMultiplierEffect, valueMultiplier);

        private void FruitDestroy(Fruit currentFruit) {
            _soundService.PlaySoundMerge();
            _scoreCounter.AddScore(currentFruit.FruitScore);
            _assetProvider.Unload(currentFruit.gameObject);
        }

        private void ExplodeWatermelon(Fruit watermelon) {
            _coroutineRunner.StartCoroutine(ScaleWatermelon(watermelon.gameObject));
        }

        private IEnumerator DelayForAction(float delayInSecond, UnityAction unityEvent) {
            yield return new WaitForSeconds(delayInSecond);
            unityEvent?.Invoke();
        }

        private IEnumerator ScaleWatermelon(GameObject gameObject) {
            yield return new WaitForSeconds(1);
            float startScaleX = gameObject.transform.localScale.x;
            startScaleX += 0.1f;
            while(startScaleX > gameObject.transform.localScale.x){
                yield return null;
                gameObject.transform.localScale += new Vector3(0.001f, 0.001f, 0.001f);
            }
            _effectsFactory.CreateExplosionWithSmoke(gameObject.transform.position);
            _effectsFactory.CreateCrystalAddEffect(gameObject.transform.position);

            _assetProvider.Unload(gameObject);
            _sessionData.numberOfCrystal += 1;
            _viewProvider.gameSceneView.CrystalScore.text = _sessionData.numberOfCrystal.ToString();
            
        }
    }
}