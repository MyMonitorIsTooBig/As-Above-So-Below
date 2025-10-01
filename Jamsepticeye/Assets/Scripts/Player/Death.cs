using Unity.VisualScripting;
using UnityEngine;

public class Death : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;

    int _health = 3;
    bool _invincible = false;
    bool _projInvincible = false;

    [SerializeField] Upgrade _currentUpgrade;

    // initializes corpse
    private void OnEnable()
    {
        CorpseManager.Instance.AddToList(this);

        switch (_currentUpgrade)
        {
            case Upgrade.ZeroG:
                _rb.gravityScale = 0;
                _rb.bodyType = RigidbodyType2D.Kinematic;
                break;

            case Upgrade.Floating:

                break;

            case Upgrade.LongLasting:
                _invincible = true;
                break;

            case Upgrade.LeadLined:
                _projInvincible = true;
                break;
        }
    }

    // checks if colliding with player and if player's bottom is touching this corpse
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            foreach(ContactPoint2D contact in collision.contacts)
            {
                if(contact.normal.y < 0)
                {
                    _health--;

                    if(_health <= 0 && !_invincible)
                    {
                        DeleteCorpse();
                    }

                    return;
                }
            }
        }

        if (collision.collider.CompareTag("Projectile") && !_projInvincible)
        {
            DeleteCorpse();
        }
    }

    // properly disposes of corpse
    public void DeleteCorpse()
    {
        CorpseManager.Instance.RemoveFromlist(this);
        Destroy(gameObject);
    }
}

public enum Upgrade
{
    LongLasting, ZeroG, Floating, LeadLined, None,
}
