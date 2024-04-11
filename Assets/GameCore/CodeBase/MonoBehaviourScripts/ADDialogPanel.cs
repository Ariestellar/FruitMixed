using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ADDialogPanel : MonoBehaviour
{
    [field: SerializeField] public Button ADButton { get; private set; }
    [field: SerializeField] public Button CrystalButton { get; private set; }
    [field: SerializeField] public Button CloseButton { get; private set; }
    [field: SerializeField] public TextMeshProUGUI CostInCrystals { get; private set; }
}
