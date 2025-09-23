using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public abstract class Attack : MonoBehaviour
{

    //abstract class inhereted by the attack objects
    [SerializeField]
    protected float enemyRadius = 10f;

    [SerializeField] protected float attackRadius = 5f;

    protected bool canMove = true;
    protected bool _autoAttack = true;
    protected bool _canAttack = true;

    [SerializeField] protected bool lockAttackAngle = false;

    Vector3 vel = Vector3.zero;
    Vector3 smoothDirection;
    float smoothing = 0.1f;

    protected PlayerStats _stats;
    protected PlayerMovement _pm;


    InputAction _atkBtn;

    public delegate void OnAttack(bool attackState);
    public static OnAttack onAttack;


    public abstract void attack();
    public virtual void DirectionalAttack(Vector2 direction) { }

    private void Awake()
    {
        _stats = GetComponentInParent<PlayerStats>();    
        _pm = GetComponentInParent<PlayerMovement>();
        
    }

    protected virtual void OnEnable()
    {
        _atkBtn = InputSystem.actions.FindAction("Attack");

        _atkBtn.performed += attackButton;
        
        _canAttack = true;
    }

    private void OnDisable()
    {
        _atkBtn.performed -= attackButton;
    }

    void attackButton(InputAction.CallbackContext context)
    {
        if (_canAttack)
        {
            StartCoroutine(autoAttack());
        }
    }


    

    private void Update()
    {
        if (lockAttackAngle)
        {
            if (!canMove)
            {
                return;
            }

        }



        /*

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, enemyRadius);

        GameObject closest = null;
        float closestDist = enemyRadius;

        foreach (var enemy in nearbyEnemies)
        {

            if (enemy.gameObject != this && enemy.CompareTag("Enemy"))
            {
                if (enemy == null) continue;
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance < closestDist)
                {
                    closestDist = distance;
                    closest = enemy.gameObject;
                }
            }
        }

        if (closest != null)
        {
            if(closestDist <= attackRadius && _autoAttack && _canAttack)
            {
                //StartCoroutine(autoAttack());
            }


            smoothDirection = Vector3.SmoothDamp(smoothDirection, closest.transform.position - transform.position, ref vel, smoothing);
            //DirectionalAttack(smoothDirection);
            return;
        }

        

        */

        smoothDirection = Vector3.SmoothDamp(smoothDirection, _pm.Direction, ref vel, smoothing);
        //DirectionalAttack(_pm.Direction);
        DirectionalAttack(smoothDirection);
    }


    IEnumerator autoAttack()
    {
        
        _canAttack = false;
        onAttack?.Invoke(_canAttack);
        attack();
        yield return new WaitForSeconds(_stats.atkSpeed.value);
        
        _canAttack = true;
        

    }

}
