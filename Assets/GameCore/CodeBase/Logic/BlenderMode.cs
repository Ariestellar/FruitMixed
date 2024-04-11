using System.Collections;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using UnityEngine;
using UnityEngine.Events;
using static BlenderAnimation;

public class BlenderMode: IHelpmode
{
    private const ObjectInHand OBJECT = ObjectInHand.Blender;
    private const int COST_IN_CRYSTAL = 2;
    private ADDialog _adDialog;
    private ICoroutineRunner _coroutineRunner;
    private ToggleModeView _toggleModeView;
    private InputService _inputService;
    private EffectsFactory _effectsFactory;
    private AssetProvider _assetProvider;
    private SessionData _sessionData;
    private BlenderAnimation _blenderAnimator;
    private UnityEvent<bool, IHelpmode> _modeSwitched = new UnityEvent<bool, IHelpmode>();
    private UnityEvent<Fruit> _fruitDestroyed = new UnityEvent<Fruit>();
    private bool _isRelease;
    public UnityEvent<bool, IHelpmode> ModeSwitched { get => _modeSwitched; }
    public UnityEvent<Fruit> FruitDestroyed { get => _fruitDestroyed; }
    public bool IsRelease  => _isRelease;
    public ObjectInHand ObjectInHand => OBJECT;

    public BlenderMode(ICoroutineRunner coroutineRunner,
        ADDialog adDialog, 
        ToggleModeView ToggleModeView, 
        InputService inputService,
        EffectsFactory effectsFactory,
        AssetProvider assetProvider, 
        SessionData sessionData) {
        _adDialog = adDialog;
        _coroutineRunner = coroutineRunner;
        _toggleModeView = ToggleModeView;
        _inputService = inputService;
        _effectsFactory = effectsFactory;
        _assetProvider = assetProvider;
        _sessionData = sessionData;

        ChangeNumber(_sessionData.numberOfBlend);

        _inputService.AddInactiveTouchZone(_toggleModeView.Toggle.GetComponent<InactiveTouchZone>());
        _inputService.AddInactiveTouchZone(_toggleModeView.ADDisplayButton.GetComponent<InactiveTouchZone>());        
        _toggleModeView.Toggle.onValueChanged.AddListener(SwitchMode);
        _toggleModeView.ADDisplayButton.onClick.AddListener(HandlerADShow);
        _adDialog.ObjectGetting.AddListener(ViewingAds);
    }

    private void HandlerADShow() => _adDialog.ShowADDialogPanel(ObjectInHand.Blender, COST_IN_CRYSTAL);

    private void ViewingAds(ObjectInHand supportingObject) {
        if(supportingObject == ObjectInHand.Blender){
            _sessionData.numberOfBlend += 1;
            _toggleModeView.CountAvailableObject.text = _sessionData.numberOfBlend.ToString();
            _toggleModeView.Toggle.interactable = true;
            _toggleModeView.ADDisplayButton.gameObject.SetActive(false);
        }        
    }

    private void SwitchMode(bool value) { 
        if(value) ShowBlender();
        else HideBlender(); 
    }

    private void HideBlender() { 
        _isRelease = false;
        _modeSwitched?.Invoke(false, this);
        _toggleModeView.Toggle.interactable = false;
        _blenderAnimator.SetTrigger(BlenderTrigger.Hide);
        _inputService.TouchPointUped.RemoveListener(StartAnimation);
        _coroutineRunner.StartCoroutine(DelayForAction(1, DestroyBlender));
    }

    private void DestroyBlender() {
        _blenderAnimator.FruitDestroyed.RemoveListener(DestroyFruit);
        _assetProvider.Unload(_blenderAnimator.gameObject);
        _toggleModeView.Toggle.interactable = true;
        _toggleModeView.Toggle.SetIsOnWithoutNotify(false);
        
    }

    private async void ShowBlender()
    {
        _modeSwitched?.Invoke(true, this);
        _blenderAnimator = await _effectsFactory.CreateBlenderAnimation();
        _inputService.TouchPointUped.AddListener(StartAnimation);
    }

    private void StartAnimation() {
        _isRelease = true;
        _toggleModeView.Toggle.interactable = false;
        _blenderAnimator.SetTrigger(BlenderTrigger.BlendStart);
        _inputService.TouchPointUped.RemoveListener(StartAnimation); 
        _coroutineRunner.StartCoroutine(DelayForAction(5, DestroyAfterAnimationBlender));
        _blenderAnimator.FruitDestroyed.AddListener(DestroyFruit);
        _sessionData.numberOfBlend -= 1;
    }

    private void DestroyAfterAnimationBlender() {
        _modeSwitched?.Invoke(false, this);
        DestroyBlender();
        ChangeNumber(_sessionData.numberOfBlend);
    }

    private void ChangeNumber(int newNumber) {
        _toggleModeView.CountAvailableObject.text = newNumber.ToString();
        if(newNumber == 0) ShowButtonForAD();
    }

    private void ShowButtonForAD() {
        _toggleModeView.Toggle.interactable = false;
        _toggleModeView.ADDisplayButton.gameObject.SetActive(true);
    }

    private IEnumerator DelayForAction(float delayInSecond, UnityAction unityEvent) {
        yield return new WaitForSeconds(delayInSecond);
        unityEvent?.Invoke();
    }

    private void DestroyFruit(Fruit fruit) => _fruitDestroyed?.Invoke(fruit);

    public void SwitchControlDropp(bool value) {
        if(value){ 
            _inputService.TouchPointUped.RemoveListener(StartAnimation);
        } else {
            if(_isRelease == false) {
                _inputService.TouchPointUped.AddListener(StartAnimation);
            }
        }
    }
}
