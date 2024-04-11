using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Bomb : MonoBehaviour
{    
    [SerializeField] private float _limitMovementHorizontally;
    private UnityEvent<Bomb> _released = new UnityEvent<Bomb>(); 
    private UnityEvent<Bomb, Vector3, List<Fruit>> _explosioned = new UnityEvent<Bomb, Vector3, List<Fruit>>();
    private Rigidbody2D _rigidbody2D;
    private LayerMask _fruitLayerMask;
    private bool _isRelease;
    public UnityEvent<Bomb> Released { get => _released; }
    public UnityEvent<Bomb, Vector3, List<Fruit>> Explosioned { get => _explosioned; }
    public bool IsRelease { get => _isRelease; }

    private void Awake() {
        _fruitLayerMask = LayerMask.GetMask("Fruit");
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Move(Vector2 screenTouchPointPosition) {
        transform.position = new Vector3(Mathf.Clamp(screenTouchPointPosition.x, -_limitMovementHorizontally, _limitMovementHorizontally),
        transform.position.y, this.gameObject.transform.position.z);
    }

    public void Release() { 
        _isRelease = true;
        _rigidbody2D.gravityScale = 2f;
        _released?.Invoke(this);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag.Contains("Floor") || (collision.gameObject.TryGetComponent(out Fruit collisionFruit) && collisionFruit.FruitState != FruitState.Ready)) {
            List<Fruit> fruits = new List<Fruit>();            
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2, _fruitLayerMask);
            for (int i = 0; i < hitColliders.Length; i++) {
                if(hitColliders[i].TryGetComponent(out Fruit fruit)) fruits.Add(fruit);
            }
            _explosioned?.Invoke(this, transform.position, fruits);
        }
    }
}

