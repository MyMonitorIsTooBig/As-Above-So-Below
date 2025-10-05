using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.Image;

public class PlayerMovement : MonoBehaviour
{
    InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _dieAction;

    System.Action<InputAction.CallbackContext> _jumpCallback;
    System.Action<InputAction.CallbackContext> _dieCallback;
    System.Action<InputAction.CallbackContext> _moveCallback;

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


    List<Collider2D> _groundColliders = new List<Collider2D>();

    private void Awake()
    {
        //initialize player variables
        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _dieAction = InputSystem.actions.FindAction("Die");

        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();

        _jumpCallback = ctx => Jump();
        _dieCallback = ctx => Die();

        _jumpAction.started += _jumpCallback;
        _dieAction.started += _dieCallback;
    }
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        _jumpAction.started -= _jumpCallback;
        _dieAction.started -= _dieCallback;
    }

    void FixedUpdate()
    {
        
        if (_canMove)
        {
           
            var moveValue = _moveAction.ReadValue<Vector2>();
            moveValue.y = 0;

            dir2 = Vector2.Lerp(dir2, moveValue, _smoothing);

            if (!_grounded) dir2 = dir2 * 0.85f;


            RaycastHit2D slopeHit = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.5f), 0, Vector2.down, 1, _groundLayer);

            Vector2 slope = Vector2.one;

            if (slopeHit.collider != null)
            {
                float angle = Mathf.Atan2(slopeHit.normal.x, slopeHit.normal.y);

                if (angle != 0 && slopeHit.collider.CompareTag("Slope"))
                {
                    angle = Mathf.Deg2Rad * angle;
                    slope = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                    _rb.AddForce(dir2 * slope * _stats.speed.value, ForceMode2D.Impulse);

                }
            }

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
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(0.99f, 0.5f), 0, Vector2.down, 1, _groundLayer);


        if (_collider.IsTouchingLayers(_groundLayer) && hit.collider != null && _grounded)
        {
            _rb.AddForce(transform.up * _stats.jumpHeight.value, ForceMode2D.Impulse);
            _grounded = false;
        }
    }

    void Die()
    {
        if (CardManager.Instance.HasBaseCard)
        {
            _rewind.OnRewindPressed();
        }
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
