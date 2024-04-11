using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace CodeBase.Infrastructure.Services
{
    public class InputService : IService
    {
        private Input _input;
        public InputService() => _input = new Input();
        private UnityEvent<Vector2> _screenTouchPointPositionChanged = new UnityEvent<Vector2>();
        private UnityEvent _touchPointDowned = new UnityEvent();
        private UnityEvent _touchPointUped = new UnityEvent();
        private List<InactiveTouchZone> _inactiveTouchZones = new List<InactiveTouchZone>(); 
        public UnityEvent<Vector2> ScreenTouchPointPositionChanged { get => _screenTouchPointPositionChanged; }
        public UnityEvent TouchPointUped { get => _touchPointUped; }
        public UnityEvent TouchPointDowned { get => _touchPointDowned; }

        public void Enable() {
            _input.Enable();
            _input.Player.PressTouchPoint.performed += TouchPointDown;
            _input.Player.PressTouchPoint.canceled += TouchPointUp;
            _input.Player.ScreenTouchPointPosition.performed += ScreenTouchPointPosition;
        }

        public void Disable()
        {
            _input.Player.PressTouchPoint.performed -= TouchPointDown;
            _input.Player.PressTouchPoint.canceled -= TouchPointUp;
            _input.Player.ScreenTouchPointPosition.performed -= ScreenTouchPointPosition;
            _input.Disable();
        }

        public void AddInactiveTouchZone(InactiveTouchZone inactiveTouchZone) {
           if(_inactiveTouchZones.Contains(inactiveTouchZone) == false) _inactiveTouchZones.Add(inactiveTouchZone);
        }

        private void TouchPointDown(InputAction.CallbackContext context) 
            => _touchPointDowned?.Invoke();

        private void TouchPointUp(InputAction.CallbackContext context) { 
            if(IsInactiveZone() == false) _touchPointUped?.Invoke();
        }

        private void ScreenTouchPointPosition(InputAction.CallbackContext context) {
            if(IsInactiveZone() == false) _screenTouchPointPositionChanged?.Invoke(Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>()));
        }

        private bool IsInactiveZone() {
            bool result = false;
            if(_inactiveTouchZones.Count == 0) return false;
            else{
                foreach (var item in _inactiveTouchZones) {
                    if(item.IsPress) return true;
                }
            }
            return result;
        }
    }
}