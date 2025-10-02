using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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


    Vector2 dir2 = Vector2.zero;

    [SerializeField] Vector2 _xMomentum = Vector2.zero;

    [SerializeField]
    Collider2D _collider;

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



    }

    void Jump()
    {
        if (_collider.IsTouchingLayers(_groundLayer))
        {
            _rb.AddForce(transform.up * _stats.jumpHeight.value, ForceMode2D.Impulse);
            _grounded = false;
        }
    }

    void Die()
    {
        Death _corpse = Instantiate(_corpsePrefab, transform.position, Quaternion.identity).GetComponent<Death>();
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
    }

    private void OnCollisionExit2D(Collision2D collision)
    {

        if (_groundColliders.Contains(collision.collider))
        {
            _groundColliders.Remove(collision.collider);
            if (_groundColliders.Count == 0) _grounded = false;
        }
    }


}
