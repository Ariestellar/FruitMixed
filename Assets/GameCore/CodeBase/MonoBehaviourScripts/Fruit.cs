using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class Fruit : MonoBehaviour
{
    [SerializeField] private FruitType _fruitType;
    [SerializeField] private int _fruitScore;
    [SerializeField] private float _limitMovementFruitsHorizontally;
    private UnityEvent<Fruit> _released = new UnityEvent<Fruit>();
    private UnityEvent<Fruit, Fruit> _collisionWithAnotherFruitOccurred = new UnityEvent<Fruit, Fruit>();    
    private FruitState _fruitState;
    
    private float _defaultScaleX;
    private Rigidbody2D _rigidbody2D;
    public UnityEvent<Fruit> Released { get => _released; }
    public UnityEvent<Fruit, Fruit> CollisionWithAnotherFruitOccurred { get => _collisionWithAnotherFruitOccurred; }
    public FruitState FruitState { get => _fruitState; }
    public FruitType FruitType { get => _fruitType; }
    public int FruitScore { get => _fruitScore; }

    public void Construct(FruitState fruitState)
    {
        this._fruitState = fruitState;
        _defaultScaleX = this.transform.localScale.x;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        
        if(FruitState.Collision == fruitState) {
            _rigidbody2D.gravityScale = 1f; 
            StartCoroutine(IncreaseScaleAfterSpawnWithCombination());
        }else if(FruitState.Ready == fruitState) {
            _rigidbody2D.isKinematic = true;
            _rigidbody2D.simulated = false;
        }
    }

    public void Move(Vector2 screenTouchPointPosition) {
        transform.position = new Vector3(Mathf.Clamp(screenTouchPointPosition.x, -_limitMovementFruitsHorizontally, _limitMovementFruitsHorizontally),
        transform.position.y, this.gameObject.transform.position.z);
    }

    public void Release() { 
        _rigidbody2D.isKinematic = false;
        _rigidbody2D.simulated = true;
        _rigidbody2D.gravityScale = 2f;
        _fruitState = FruitState.Dropping;
        _released?.Invoke(this);
    }

    private IEnumerator IncreaseScaleAfterSpawnWithCombination() {
        this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        while(this.transform.localScale.x < _defaultScaleX) {
            yield return null;
            this.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag.Contains("Wall") || collision.gameObject.tag.Contains("Floor") || collision.gameObject.GetComponent<Fruit>()) {
            _fruitState = FruitState.Collision;
        }

        if(collision.gameObject.TryGetComponent(out Fruit fruit)) {
            if (_fruitState >= FruitState.Dropping && (_fruitType == fruit._fruitType || fruit._fruitType == FruitType.Multifruit) && _fruitType!=FruitType.Watermelon) { 
                MergeWithAnotherFruit(collision.gameObject.GetComponent<Fruit>());
            }
        } 
    }

    private void MergeWithAnotherFruit(Fruit anotherFruit) {        
        if(this.gameObject.GetInstanceID() > anotherFruit.GetInstanceID() || anotherFruit.FruitType == FruitType.Multifruit) {
            _collisionWithAnotherFruitOccurred.Invoke(this, anotherFruit);
        }
    }
}