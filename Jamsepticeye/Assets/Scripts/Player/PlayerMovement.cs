using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _interactAction;
    InputAction _attackAction;

    private Rigidbody2D _rb;

    PlayerStats _stats;

    Vector3 _vel = Vector3.zero;
    Vector3 _atkvel = Vector3.zero;

    [SerializeField]
    float _smoothing = 0.3f;

    [SerializeField]
    float _atksmoothing = 0.0f;

    [SerializeField] bool _canMove = true;
    public bool CanMove {  get { return _canMove; } set { _canMove = value; } }

    Attack _attack;

    Vector2 direction;

    public Vector2 Direction { get { return direction; } }

    bool _autoAttack = true;

    [SerializeField]
    bool _bounce = false;

    [SerializeField]
    bool _grounded = false;


    Vector2 dir2 = Vector2.zero;

    [SerializeField] Vector2 _xMomentum = Vector2.zero;

    [SerializeField]
    Collider2D _collider;

    [SerializeField]
    LayerMask _groundLayer;

    private void Awake()
    {
        //initialize player variables
        _interactAction = InputSystem.actions.FindAction("Interact");
        _moveAction = InputSystem.actions.FindAction("Move");
        _attackAction = InputSystem.actions.FindAction("Attack");
        _jumpAction = InputSystem.actions.FindAction("Jump");

        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
        _attack = GetComponentInChildren<Attack>();

        _jumpAction.started += Jump;
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        _jumpAction.started -= Jump;
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

            direction = Vector3.SmoothDamp(direction, (cam - transform.position).normalized, ref _atkvel, _atksmoothing);

            if(_rb.linearVelocity != Vector2.zero) _xMomentum = _rb.linearVelocity;
        }



    }

    void Jump(InputAction.CallbackContext context)
    {
        if (_collider.IsTouchingLayers(_groundLayer))
        {
            _rb.AddForce(transform.up * _stats.jumpHeight.value, ForceMode2D.Impulse);
            _grounded = false;
        }
    }

    void disableMove(bool enable)
    {
        _canMove = !enable;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.layer == 3)
        {
            _grounded = true;
            if(_bounce) _rb.AddForce(-_xMomentum + _rb.linearVelocity, ForceMode2D.Impulse);
            
        }

    }

}
