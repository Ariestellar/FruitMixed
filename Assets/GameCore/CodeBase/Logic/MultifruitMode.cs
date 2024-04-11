using System.Threading.Tasks;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using UnityEngine;
using UnityEngine.Events;

public class MultifruitMode : IHelpmode
{
    private const ObjectInHand OBJECT = ObjectInHand.Multifruit;
    private const int COST_IN_CRYSTAL = 3;
    private ADDialog _adDialog;
    private ToggleModeView _toggle;
    private InputService _inputService;
    private GameObjectFactory _gameObjectFactory;
    private AssetProvider _assetProvider;
    private Vector3 _spawnFruitPositon;
    private SessionData _sessionData;
    private Fruit _multifruit;
    private UnityEvent<Fruit> _fruitDestroyed = new UnityEvent<Fruit>();    

    public bool IsRelease { get => _multifruit.FruitState == FruitState.Dropping; }
    public UnityEvent<bool, IHelpmode> _modeSwitched = new UnityEvent<bool, IHelpmode>();
    public UnityEvent<bool, IHelpmode> ModeSwitched { get => _modeSwitched; }    
    public UnityEvent<Fruit> FruitDestroyed { get => _fruitDestroyed; }

    public ObjectInHand ObjectInHand => OBJECT;

    public MultifruitMode(ADDialog adDialog, 
        ToggleModeView toggle, 
        InputService inputService,
        GameObjectFactory gameObjectFactory,
        AssetProvider assetProvider, 
        Vector3 spawnFruitPositon,
        SessionData sessionData) {
        _adDialog = adDialog;
        _toggle = toggle;
        _inputService = inputService;
        _gameObjectFactory = gameObjectFactory;
        _assetProvider = assetProvider;
        _spawnFruitPositon = spawnFruitPositon;
        _sessionData = sessionData;

        ChangeNumber(_sessionData.numberOfMultifruit);

        _inputService.AddInactiveTouchZone(_toggle.Toggle.GetComponent<InactiveTouchZone>());
        _inputService.AddInactiveTouchZone(_toggle.ADDisplayButton.GetComponent<InactiveTouchZone>());        
        _toggle.Toggle.onValueChanged.AddListener(SwitchMode);
        _toggle.ADDisplayButton.onClick.AddListener(HandlerADShow);
        _adDialog.ObjectGetting.AddListener(ViewingAds);
    }    

    private void HandlerADShow() => _adDialog.ShowADDialogPanel(ObjectInHand.Multifruit, COST_IN_CRYSTAL);

    private void ViewingAds(ObjectInHand supportingObject) {
        if(supportingObject == ObjectInHand.Multifruit){
            _sessionData.numberOfMultifruit += 3;
            _toggle.CountAvailableObject.text = _sessionData.numberOfMultifruit.ToString();
            _toggle.Toggle.interactable = true;
            _toggle.ADDisplayButton.gameObject.SetActive(false);
        } 
    }

    private async void SwitchMode(bool value) {
        if(value){
            _multifruit = await GetFruit(_spawnFruitPositon);
            _multifruit.Released.AddListener(DroppFruit);
            _modeSwitched?.Invoke(value, this);
        }else{
            if(_multifruit && _multifruit.FruitState == FruitState.Ready) _assetProvider.Unload(_multifruit.gameObject);
            _modeSwitched?.Invoke(value, this);
        }
    }

    private async Task<Fruit> GetFruit(Vector3 spawnPosition) {
        Fruit multifruit = await _gameObjectFactory.CreateFruit(FruitType.Multifruit, spawnPosition, Quaternion.identity, FruitState.Ready);
        _inputService.ScreenTouchPointPositionChanged.AddListener(multifruit.Move);
        _inputService.TouchPointUped.AddListener(multifruit.Release);
        return multifruit;
    } 

    private void DroppFruit(Fruit fruit) {
        _inputService.ScreenTouchPointPositionChanged.RemoveListener(fruit.Move);
        _inputService.TouchPointUped.RemoveListener(fruit.Release);
        _toggle.Toggle.isOn = false;
        _sessionData.numberOfMultifruit -= 1;
        ChangeNumber( _sessionData.numberOfMultifruit);
    }

    private void ChangeNumber(int newNumber) {
        _toggle.CountAvailableObject.text = newNumber.ToString();
        if(newNumber == 0) ShowButtonForAD();
    }

    private void ShowButtonForAD() {
        _toggle.Toggle.interactable = false;
        _toggle.ADDisplayButton.gameObject.SetActive(true);
    }

    public void SwitchControlDropp(bool value) {
        if(value){ 
            _inputService.ScreenTouchPointPositionChanged.RemoveListener(_multifruit.Move);
            _inputService.TouchPointUped.RemoveListener(_multifruit.Release);
        } else {
            if(_multifruit.FruitState == FruitState.Ready) {
                _inputService.ScreenTouchPointPositionChanged.AddListener(_multifruit.Move);
                _inputService.TouchPointUped.AddListener(_multifruit.Release);
            }
        }
    }
}



