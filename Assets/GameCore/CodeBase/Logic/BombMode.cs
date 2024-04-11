using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using UnityEngine;
using UnityEngine.Events;

public class BombMode : IHelpmode
{
    private const ObjectInHand OBJECT = ObjectInHand.Bomb;
    private const int COST_IN_CRYSTAL = 2;
    private ADDialog _adDialog;
    private ToggleModeView _toggleBombMode;
    private InputService _inputService;
    private GameObjectFactory _gameObjectFactory;
    private EffectsFactory _effectsFactory;
    private AssetProvider _assetProvider;
    private Vector3 _spawnBombPositon;
    private SessionData _sessionData;
    private Bomb _bomb;
    private UnityEvent<Fruit> _fruitDestroyed = new UnityEvent<Fruit>();
    private UnityEvent<bool, IHelpmode> _modeSwitched = new UnityEvent<bool, IHelpmode>();
    
    public UnityEvent<bool, IHelpmode> ModeSwitched { get => _modeSwitched; }
    public UnityEvent<Fruit> FruitDestroyed { get => _fruitDestroyed; }
    public bool IsRelease => _bomb.IsRelease;
    public ObjectInHand ObjectInHand => OBJECT;

    public BombMode(ADDialog adDialog, 
        ToggleModeView toggleBombMode, 
        InputService inputService,
        GameObjectFactory gameObjectFactory,
        EffectsFactory effectsFactory,
        AssetProvider assetProvider, 
        Vector3 spawnBombPositon,
        SessionData sessionData) {
        _adDialog = adDialog;
        _toggleBombMode = toggleBombMode;
        _inputService = inputService;
        _gameObjectFactory = gameObjectFactory;
        _effectsFactory = effectsFactory;
        _assetProvider = assetProvider;
        _spawnBombPositon = spawnBombPositon;
        _sessionData = sessionData;

        ChangeNumber(_sessionData.numberOfBomb);

        _inputService.AddInactiveTouchZone(_toggleBombMode.Toggle.GetComponent<InactiveTouchZone>());
        _inputService.AddInactiveTouchZone(_toggleBombMode.ADDisplayButton.GetComponent<InactiveTouchZone>());

        _toggleBombMode.Toggle.onValueChanged.AddListener(SwitchBombMode);
        _toggleBombMode.ADDisplayButton.onClick.AddListener(HandlerADShow);

        _adDialog.ObjectGetting.AddListener(ViewingAds);
    }

    public void SwitchControlDropp(bool value) {
        if(value){ 
            _inputService.ScreenTouchPointPositionChanged.RemoveListener(_bomb.Move);
            _inputService.TouchPointUped.RemoveListener(_bomb.Release);
        } else {
            if(_bomb.IsRelease == false) {
                _inputService.ScreenTouchPointPositionChanged.AddListener(_bomb.Move);
                _inputService.TouchPointUped.AddListener(_bomb.Release);
            }
        }
    }

    private void HandlerADShow() => _adDialog.ShowADDialogPanel(ObjectInHand.Bomb, COST_IN_CRYSTAL);
    private void ViewingAds(ObjectInHand supportingObject) {
        if(supportingObject == ObjectInHand.Bomb){
            _sessionData.numberOfBomb += 2;
            _toggleBombMode.CountAvailableObject.text = _sessionData.numberOfBomb.ToString();
            _toggleBombMode.Toggle.interactable = true;
            _toggleBombMode.ADDisplayButton.gameObject.SetActive(false);
        }        
    }

    private async void SwitchBombMode(bool value) {
        if(value){
            _bomb = await GetBomb(_spawnBombPositon);
            _bomb.Released.AddListener(DroppBomb);
            _modeSwitched?.Invoke(value, this);
        }else{
            if(_bomb && _bomb.IsRelease == false) _assetProvider.Unload(_bomb.gameObject);
            _modeSwitched?.Invoke(value, this);
        }
    }

    private async Task<Bomb> GetBomb(Vector3 spawnPosition) {
        Bomb bomb = await _gameObjectFactory.CreateBomb(spawnPosition, Quaternion.identity);
        _inputService.ScreenTouchPointPositionChanged.AddListener(bomb.Move);
        _inputService.TouchPointUped.AddListener(bomb.Release);
        return bomb;
    } 

    private void DroppBomb(Bomb bomb) {
        _inputService.ScreenTouchPointPositionChanged.RemoveListener(bomb.Move);
        _inputService.TouchPointUped.RemoveListener(bomb.Release);
        bomb.Explosioned.AddListener(ExplosionedBomb);
        _toggleBombMode.Toggle.isOn = false;
        _sessionData.numberOfBomb -= 1;
        ChangeNumber( _sessionData.numberOfBomb);
        
    }

    private async void ExplosionedBomb(Bomb bomb, Vector3 explosionePosition, List<Fruit> fruitsInAffectedArea) {
        _assetProvider.Unload(bomb.gameObject);
        await _effectsFactory.CreateExplosionWithFire(explosionePosition);
        fruitsInAffectedArea.ForEach(fruit => DestroyFruit(fruit));
        fruitsInAffectedArea = new List<Fruit>();
    }

    private void ChangeNumber(int newNumber) {
        _toggleBombMode.CountAvailableObject.text = newNumber.ToString();
        if(newNumber == 0) ShowButtonForAD();
    }

    private void ShowButtonForAD() {
        _toggleBombMode.Toggle.interactable = false;
        _toggleBombMode.ADDisplayButton.gameObject.SetActive(true);
    }

    private void DestroyFruit(Fruit fruit) => _fruitDestroyed?.Invoke(fruit);
}