using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class PlayerMovement : MonoBehaviour
{
    InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _dieAction;

    private Rigidbody2D _rb;

    PlayerStats _stats;

    Vector3 _vel = Vector3.zero;

    [SerializeField]
    float _smoothing = 0.3f;


    [SerializeField] bool _canMove = true;
    public bool CanMove {  get { return _canMove; } set { _canMove = value; } }


    Vector2 direction;

    public Vector2 Direction { get { return direction; } }


    [SerializeField]
    bool _grounded = false;

    bool _inWater = false;

    [SerializeField] float _timeUntilWateryGrave = 1.0f;
    float _currentWateryGraveTime = 0.0f;


    Vector2 dir2 = Vector2.zero;

    [SerializeField] Vector2 _xMomentum = Vector2.zero;

    [SerializeField]
    Collider2D _collider;
    [SerializeField]
    PlayerRewind _rewind;

    [SerializeField]
    LayerMask _groundLayer;

    [SerializeField]
    GameObject _corpsePrefab;

    List<Collider2D> _groundColliders = new List<Collider2D>();

    private void Awake()
    {
        //initialize player variables
        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _dieAction = InputSystem.actions.FindAction("Die");

        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();

        _jumpAction.started += ctx => Jump();
        _dieAction.started += ctx => Die();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        _jumpAction.started -= ctx => Jump();
        _dieAction.started -= ctx => Die();
    }

    void FixedUpdate()
    {
        
        if (_canMove)
        {
           
            var moveValue = _moveAction.ReadValue<Vector2>();
            moveValue.y = 0;

            dir2 = Vector2.Lerp(dir2, moveValue, _smoothing);

            if (!_grounded) dir2 = dir2 * 0.85f;

            _rb.AddForce(dir2 * _stats.speed.value, ForceMode2D.Impulse);

            Vector3 cam = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());


        }

        if (_inWater)
        {
            if(_currentWateryGraveTime < _timeUntilWateryGrave)
            {
                _currentWateryGraveTime += Time.deltaTime;
            }
            else
            {
                Die();
                _currentWateryGraveTime = 0.0f;
            }
        }

    }

    void Jump()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 1f), 0, Vector2.down, 1, _groundLayer);


        if (_collider.IsTouchingLayers(_groundLayer) && hit.collider != null)
        {
            _rb.AddForce(transform.up * _stats.jumpHeight.value, ForceMode2D.Impulse);
            _grounded = false;
        }
    }

    void Die()
    {
        Death _corpse = Instantiate(_corpsePrefab, transform.position, Quaternion.identity).GetComponent<Death>();
        _rewind.OnRewindPressed();
        //_corpse.CurrentUpgrade = Upgrade.LongLasting;
    }

    void disableMove(bool enable)
    {
        _canMove = !enable;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        foreach (ContactPoint2D contact in collision.contacts)
        {

            float ownBottomY = _collider.bounds.min.y;

            float tolerance = 0.1f; 

            if (Mathf.Abs(contact.point.y - ownBottomY) < tolerance)
            {
                _grounded = true;
                _groundColliders.Add(collision.collider);
                break; 
            }
        }


        switch (collision.collider.tag)
        {
            case "Spike":
                Die();
                break;

            case "Projectile":
                Die();
                break;
        }
        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {

        if (_groundColliders.Contains(collision.collider))
        {
            _groundColliders.Remove(collision.collider);
            if (_groundColliders.Count == 0) _grounded = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 4)
        {
            _inWater = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 4)
        {
            _inWater = false;
        }
    }


}
