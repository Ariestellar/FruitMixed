using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource), typeof(Animator))]
public class BlenderAnimation : MonoBehaviour
{
    public enum BlenderTrigger{Hide, BlendStart};
    [SerializeField] private Transform _knifeCenter;
    [SerializeField] private GameObject _blockCanvas;
    private int _fruitLayerMask;
    private AudioSource _audioSource;
    private Animator _animator;
    private Coroutine _physicalPushing;
    private UnityEvent<Fruit> _fruitDestroyed = new UnityEvent<Fruit>();
    public UnityEvent<Fruit> FruitDestroyed { get => _fruitDestroyed; }

    private void Awake() {
        _fruitLayerMask = LayerMask.GetMask("Fruit");
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
    }

    public void PlaySound() => _audioSource.Play();
    public void PhysicalPushingStart() => _physicalPushing = StartCoroutine(PhysicalPushing());
    public void PhysicalPushingStop() => StopCoroutine(_physicalPushing);
    public void SetTrigger(BlenderTrigger trigger)
    {
        if(BlenderTrigger.BlendStart == trigger) _blockCanvas.gameObject.SetActive(true);
        _animator.SetTrigger(trigger.ToString());
    }

    private IEnumerator PhysicalPushing() {
        while(true) {
            yield return new WaitForSeconds(0.1f);
            Push();
        } 
    }

    private void Push() {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(_knifeCenter.position, 2, _fruitLayerMask);
        for (int i = 0; i < hitColliders.Length; i++) {
            if(hitColliders[i].TryGetComponent(out Rigidbody2D fruit)){ 
                if(Random.value <= 0.03f) _fruitDestroyed?.Invoke(fruit.GetComponent<Fruit>());
                else fruit.AddForce(new Vector2(Random.Range(100f,500f), Random.Range(100f,200f)));
            }
        }
    }
}
