using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    InputAction _moveAction;
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





    private void Awake()
    {
        //initialize player variables
        _interactAction = InputSystem.actions.FindAction("Interact");
        _moveAction = InputSystem.actions.FindAction("Move");
        _attackAction = InputSystem.actions.FindAction("Attack");

        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
        _attack = GetComponentInChildren<Attack>();


    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    void FixedUpdate()
    {
        
        if (_canMove)
        {
           

            // move player's rigidbody position based on current read input value using *new* input system

            var moveValue = _moveAction.ReadValue<Vector2>();


            var dir = Vector3.SmoothDamp(transform.position, transform.position + (Vector3)moveValue * _stats.speed.value, ref _vel, _smoothing);

            //dir = transform.position + (Vector3)moveValue * _stats.speed.value;



            _rb.MovePosition(dir);

            Vector3 cam = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            direction = Vector3.SmoothDamp(direction, (cam - transform.position).normalized, ref _atkvel, _atksmoothing);


        }



    }



    void disableMove(bool enable)
    {
        _canMove = !enable;
    }



}
