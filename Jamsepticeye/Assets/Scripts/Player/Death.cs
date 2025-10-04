using System;
using Unity.VisualScripting;
using UnityEngine;

public class Death : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;

    [SerializeField] float _health = 3;
    [SerializeField] float _timeUntilDamage = 1.0f;
    float _currentTime = 0.0f;

    [SerializeField] float _stepDamage = 1.0f;
    [SerializeField] float _projectileDamage = 0.5f;
    [SerializeField] int _lastingMultipler = 2;

    bool _invincible = false;
    bool _projInvincible = false;
    bool _floating = false;

    bool _inWater = false;
    bool _stoodOn = false;

    [SerializeField] Upgrade _currentUpgrade;
    public Upgrade CurrentUpgrade { get { return _currentUpgrade; } set { _currentUpgrade = value; } }

    // initializes corpse
    private void Start()
    {
        CorpseManager.Instance.AddToList(this);

        switch (_currentUpgrade)
        {
            case Upgrade.ZeroG:
                _rb.gravityScale = 0;
                _rb.bodyType = RigidbodyType2D.Kinematic;
                break;

            case Upgrade.Floating:
                _floating = true;
                break;

            case Upgrade.LongLasting:
                _health = _health * _lastingMultipler;
                break;

            case Upgrade.LeadLined:
                _projInvincible = true;
                break;
        }
    }

    private void Update()
    {
        if (_stoodOn)
        {
            if(_currentTime < _timeUntilDamage)
            {
                _currentTime += Time.deltaTime;
            }
            else
            {
                Damaged(_stepDamage, _invincible);
                _currentTime = 0.0f;
            }
        }
    }



    // checks if colliding with player and if player's bottom is touching this corpse
    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            // collides with player's feet it loses some health and does the destroy check
            case "Player":
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y < 0)
                    {
                        _stoodOn = true;
                        //Damaged(_stepDamage, _invincible);
                        return;
                    }
                }
                break;

            // collides with projectile and loses some health and does the projectile destroy check
            case "Projectile":
                Damaged(_projectileDamage, _projInvincible);
                break;

        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            case "Player":
                _stoodOn = false;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // does a check for if it's in water and makes it float up
        if(collision.gameObject.layer == 4 && _floating)
        {
            _inWater = true;
            _rb.gravityScale = -1;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // does a check for if it's no longer in water and makes it float back down
        if (collision.gameObject.layer == 4 && _floating)
        {
            _inWater = false;
            _rb.gravityScale = 1;
        }
    }



    // properly disposes of corpse
    public void DeleteCorpse()
    {
        CorpseManager.Instance.RemoveFromlist(this);
        Destroy(gameObject);
    }

    public void Damaged(float damage, bool damageCheck)
    {
        if (!damageCheck)
        {
            _health = _health - damage;
        }
        if (_health <= 0)
            DeleteCorpse();
        
    }
}

[Serializable]
public enum Upgrade
{
    None, LongLasting, Floating, LeadLined, ZeroG,
}
