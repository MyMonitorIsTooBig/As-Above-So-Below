using System.Collections;
using UnityEngine;

public class IAO_Punch : MonoBehaviour, IAttackObject
{
    bool canDamage = false;
    bool _cooldown = false;
    [SerializeField]int _dmg = 1;
    int _cd = 1;
    float _dmgT = 0.16f;


    PolygonCollider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<PolygonCollider2D>();
        _collider.enabled = false;
    }

    public void damage()
    {
        if (!_cooldown)
        {
            StartCoroutine(damageCheck());
            StartCoroutine(cooldown());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canDamage)
        {

            Debug.Log("Hit: " + collision.gameObject.name);

            if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable obj))
            {
                obj.TakeDamage(_dmg, transform.position);
                
            }
            if (collision.CompareTag("Enemy"))
            {
                
            }

        }

    }

    IEnumerator damageCheck()
    {
        canDamage = true;
        _collider.enabled = true;
        yield return new WaitForSeconds(_dmgT);
        _collider.enabled = false;
        canDamage = false;
    }

    IEnumerator cooldown()
    {
        _cooldown = true;
        yield return new WaitForSeconds(_dmgT*2);
        _cooldown = false;
    }
}
