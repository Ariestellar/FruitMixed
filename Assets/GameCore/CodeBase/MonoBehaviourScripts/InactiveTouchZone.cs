using UnityEngine;
using UnityEngine.EventSystems;

public class InactiveTouchZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _isPress;
    public bool IsPress { get => _isPress; }
    public void OnPointerDown(PointerEventData eventData) => _isPress = true;
    public void OnPointerUp(PointerEventData eventData) => _isPress = false;
}
