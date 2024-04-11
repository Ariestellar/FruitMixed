using UnityEngine;
using UnityEngine.UI;

public class SoundSceneComponents : MonoBehaviour
{
    [field: SerializeField] public Toggle MuteSoundToggle { get; private set; }
    [field: SerializeField] public AudioSource AudioSourceBackground { get; private set; }
    [field: SerializeField] public AudioSource AudioSourceMerge { get; private set; }
}
