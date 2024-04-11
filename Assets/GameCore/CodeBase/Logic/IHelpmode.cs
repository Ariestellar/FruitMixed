using UnityEngine.Events;

public interface IHelpmode {
    bool IsRelease { get; }
    ObjectInHand ObjectInHand { get; }
    void SwitchControlDropp(bool value);
    UnityEvent<bool, IHelpmode> ModeSwitched { get; }
}