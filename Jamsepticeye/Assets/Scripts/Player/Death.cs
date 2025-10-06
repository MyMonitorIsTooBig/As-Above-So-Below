using System;
using Unity.VisualScripting;
using UnityEngine;

public class Death : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] Collider2D _collider;
    [SerializeField] SpriteRenderer _spriteRenderer; // NEW: assign this in Inspector

    [SerializeField] float _health = 3;
    private float _maxHealth; // NEW: store initial health

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

    private void Start()
    {
        CorpseManager.Instance.AddToList(this);

        _maxHealth = _health; // store starting health

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
                _maxHealth = _health; // update max health if modified
                break;

            case Upgrade.LeadLined:
                _projInvincible = true;
                break;
        }

        _collider.enabled = true;
        UpdateColor(); // set correct color at start
    }

    private void Update()
    {
        if (_stoodOn)
        {
            if (_currentTime < _timeUntilDamage)
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            case "Player":
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y < 0)
                    {
                        _stoodOn = true;
                        return;
                    }
                }
                break;

            case "Projectile":
                Damaged(_projectileDamage, _projInvincible);
                break;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            _stoodOn = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 4 && _floating)
        {
            _inWater = true;
            _rb.mass = 30f;
            _rb.gravityScale = -1;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 4 && _floating && _rb.gravityScale != -1)
        {
            _rb.gravityScale = -1;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 4 && _floating)
        {
            _inWater = false;
            _rb.gravityScale = 1;
        }
    }

    public void DeleteCorpse()
    {
        CorpseManager.Instance.RemoveFromlist(this);
        Destroy(gameObject);
    }

    public void Damaged(float damage, bool damageCheck)
    {
        if (!damageCheck)
        {
            _health -= damage;
            UpdateColor(); // update color when damaged
        }

        if (_health <= 0)
            DeleteCorpse();
    }

    private void UpdateColor()
    {
        if (_spriteRenderer == null) return;

        float t = Mathf.Clamp01(_health / _maxHealth);
        _spriteRenderer.color = Color.Lerp(Color.black, Color.white, t);
    }
}

[Serializable]
public enum Upgrade
{
    None, LongLasting, Floating, LeadLined, ZeroG,
}
