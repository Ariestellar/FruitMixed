using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneView : MonoBehaviour { 
    [field: SerializeField] public EndLineGame EndLineGame { get; private set; }
    [field: SerializeField] public GameObject SpawnFruitPosition { get; private set; }
    [field: SerializeField] public SoundSceneComponents SoundSceneComponents { get; private set; }
    [field: SerializeField] public TextMeshProUGUI CurrentScore { get; private set; }
    [field: SerializeField] public TextMeshProUGUI TopScore { get; private set; }
    [field: SerializeField] public TextMeshProUGUI CrystalScore { get; private set; }
    [field: SerializeField] public Toggle HelpToggle { get; private set; }
    [field: SerializeField] public ToggleModeView ToggleBombMode { get; private set; }
    [field: SerializeField] public ToggleModeView ToggleBlenderMode { get; private set; }
    [field: SerializeField] public ToggleModeView ToggleMultifruitMode { get; private set; }
    [field: SerializeField] public Transform FruitsInGlasseParent { get; private set; }
    [field: SerializeField] public Transform SpawnMultiplierEffect { get; private set; }
}