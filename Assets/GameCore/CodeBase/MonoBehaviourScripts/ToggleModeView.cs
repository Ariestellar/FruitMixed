using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleModeView : MonoBehaviour
{
    [field: SerializeField] public Toggle Toggle { get; private set; }
    [field: SerializeField] public TextMeshProUGUI CountAvailableObject { get; private set; }
    [field: SerializeField] public Button ADDisplayButton { get; private set; }
}
