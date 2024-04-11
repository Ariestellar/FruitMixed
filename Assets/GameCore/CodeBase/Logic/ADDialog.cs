using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using UnityEngine.Events;
using YG;

public class ADDialog
{
    private SoundService _soundService;
    private UIFactory _uiFactory;
    private InputService _inputService;
    private AssetProvider _assetProvider;
    private SessionData _sessionData;
    private ObjectInHand _currentSupportingObject;
    private int _costInCrystal;
    private ADDialogPanel _adDialogPanel;
    private UnityEvent<bool> _switchADDialogPanel = new UnityEvent<bool>();

    private UnityEvent _crystalsSpending = new UnityEvent();
    private UnityEvent<ObjectInHand> _objectGetting = new UnityEvent<ObjectInHand>();    
    public UnityEvent<bool> SwitchADDialogPanel { get => _switchADDialogPanel; }
    public UnityEvent<ObjectInHand> ObjectGetting { get => _objectGetting; }
    public UnityEvent CrystalsSpending { get => _crystalsSpending; }

    public ADDialog(UIFactory uiFactory, InputService inputService, AssetProvider assetProvider, SessionData sessionData, SoundService soundService)
    {
        _soundService = soundService;
        _uiFactory = uiFactory;
        _inputService = inputService;
        _assetProvider = assetProvider;
        _sessionData = sessionData;
        YandexGame.ErrorVideoEvent += ErrorRewarded;
        YandexGame.RewardVideoEvent += Rewarded;
    } 

    public async void ShowADDialogPanel(ObjectInHand supportingObject, int costInCrystal) {         
        _currentSupportingObject = supportingObject;
        _costInCrystal = costInCrystal;
        _adDialogPanel = await _uiFactory.CreateADDialogPanel();        
        _adDialogPanel.CostInCrystals.text = costInCrystal.ToString();
        _adDialogPanel.transform.GetChild(2).GetChild((int)supportingObject).gameObject.SetActive(true);
        _inputService.AddInactiveTouchZone(_adDialogPanel.ADButton.GetComponent<InactiveTouchZone>());
        _inputService.AddInactiveTouchZone(_adDialogPanel.CloseButton.GetComponent<InactiveTouchZone>());
        _adDialogPanel.ADButton.onClick.AddListener(ConfirmADShowing);

        if(_sessionData.numberOfCrystal >= _costInCrystal) {
            _adDialogPanel.CrystalButton.interactable = true;
            _adDialogPanel.CrystalButton.onClick.AddListener(BuyForCrystal);
        }else{
            _adDialogPanel.CrystalButton.interactable = false;
        }

        _adDialogPanel.CloseButton.onClick.AddListener(CancelingADViewing);
        _switchADDialogPanel?.Invoke(true);
    }

    private void Rewarded(int idAD)
    {
        _soundService.SwitchMute(_sessionData.isOffSound);
        _switchADDialogPanel?.Invoke(false);
        _objectGetting?.Invoke(_currentSupportingObject);
    }

    private void ErrorRewarded()
    {
        _soundService.SwitchMute(_sessionData.isOffSound);
        _switchADDialogPanel?.Invoke(false);
    }

    private void CancelingADViewing()
    {
        _switchADDialogPanel?.Invoke(false);
        _assetProvider.Unload(_adDialogPanel.gameObject);
    }

    private void ConfirmADShowing()
    { 
        YandexGame.RewVideoShow(0);
        _assetProvider.Unload(_adDialogPanel.gameObject);
    }

    private void BuyForCrystal() { 
        _sessionData.numberOfCrystal -= _costInCrystal;
        _crystalsSpending?.Invoke();
        _switchADDialogPanel?.Invoke(false);
        _objectGetting?.Invoke(_currentSupportingObject);
        _assetProvider.Unload(_adDialogPanel.gameObject);
    }
}