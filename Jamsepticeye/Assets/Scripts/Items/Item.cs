using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public abstract class Item : MonoBehaviour
{
    protected string _name;
    protected string _desc;
    protected int cost;
    protected bool canPickup = false;
    
    protected Sprite invSprite;

    [SerializeField] protected ItemStats _stats;
    public ItemStats Stats { get { return _stats; } }

    public string ItemName { get { return _name; } }
    public string Desc { get { return _desc; } }
    public int Cost { get { return cost; } }
    public Sprite InvSprite { get { return invSprite; } }


    private void Start()
    {
        setUpStats();
    }

    private void OnEnable()
    {
        StartCoroutine(pickupTimer());
        canPickup = false;
    }

    //sets up base stats using the set scriptable object
    public  void setUpStats()
    {

        if(_stats != null)
        {
            _name = _stats.Name;
            _desc = _stats.Description;
            cost = _stats.cost;
            invSprite = _stats.InvSprite;
        }


        initalizeStats();
    }


    //used to set up any new stats created by objects inheriting this class
    protected virtual void initalizeStats() { }

    //
    public virtual void passthruvalues() { }

    //called when the item is being held
    public virtual void onHold(bool hold) { }

    //called when the item is held and interact is done
    public virtual void interaction(IInteractable interactable)
    {

        interactable.interact(this);
    }


    IEnumerator pickupTimer()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        canPickup = false;

        if(rb != null)
        {
            //rb.AddForce(new Vector2(0, -10), ForceMode2D.Impulse);
            rb.gravityScale = 1;

            yield return new WaitForSeconds(0.33f);

            //rb.AddForce(new Vector2(0, -10), ForceMode2D.Impulse);
            rb.gravityScale = 0;
        }
        
        
        canPickup = true;

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        //checks if the player is triggering the collider and if the item can be picked up
        if (collision.gameObject.layer == 3 && canPickup)
        {
            Inventory _playerInv = collision.GetComponent<Inventory>();

            //checks if player has an inventory and if the inventory isn't full
            if (_playerInv != null)
            {
                if (!_playerInv.IsInventoryFull(this))
                {
                    _playerInv.addToInv(this);
                    Destroy(gameObject);
                }
            }
        }
    }
}
