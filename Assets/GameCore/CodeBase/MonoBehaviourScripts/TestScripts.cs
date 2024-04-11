using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TestScripts : MonoBehaviour
{
    [SerializeField] private Button _spawn;
    [SerializeField] private InputField _inputField;
    public UnityEvent<int> Spawned = new UnityEvent<int>();

    public InputField InputField { get => _inputField; }
    public Button SpawnButton { get => _spawn; }

    public void Awake() => _spawn.onClick.AddListener(Spawn);
    private void Spawn() {
        int valueFruit = _inputField.text != null?Convert.ToInt32(_inputField.text):0;
        if(valueFruit >= 0 && valueFruit <= 9) Spawned?.Invoke(valueFruit);
    }
}
