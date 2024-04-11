using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{    
    [field: SerializeField] public Button RestartButton { get; private set; }
    [SerializeField] private TextMeshProUGUI _topScore;
    [SerializeField] private TextMeshProUGUI _currentTotalScore;

    public void Construct(string topScoreValue, string currentTotalScoreValue) {
        _topScore.text = topScoreValue;
        _currentTotalScore.text = currentTotalScoreValue;
    }
}
