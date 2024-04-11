using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EndLineGame : MonoBehaviour {
    [SerializeField] private GameObject _withoutAnimation;
    [SerializeField] private GameObject _flickeringAnimation;
    private UnityEvent _timeOnLineIsOvered = new UnityEvent();
    private bool _lineHasBeenReached = false;
    private Coroutine _waitingToLeaveLine;

    public UnityEvent TimeOnLineIsOvered { get => _timeOnLineIsOvered; }

    private void OnTriggerStay2D(Collider2D collision) { 
        if(collision.gameObject.TryGetComponent(out Fruit fruit)) {
            if(_lineHasBeenReached == false) {
                if(fruit.FruitState == FruitState.Collision) {
                    _lineHasBeenReached = true;
                    _waitingToLeaveLine = StartCoroutine(WaitingToLeaveLine());
                }
            }
        }
    } 

    private void OnTriggerExit2D(Collider2D collision) { 
        if(collision.gameObject.TryGetComponent(out Fruit fruit)) {
            _lineHasBeenReached = false;
            if(_waitingToLeaveLine != null) StopCoroutine(_waitingToLeaveLine);
            SetFlikering(false);
        }
    } 

    private IEnumerator WaitingToLeaveLine() {
        SetFlikering(true);
        yield return new WaitForSeconds(3);
        SetFlikering(false);
        _timeOnLineIsOvered?.Invoke();
    }

    private void SetFlikering(bool value) {
        _flickeringAnimation.SetActive(value);
        _withoutAnimation.SetActive(!value);
    }
}
