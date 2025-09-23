using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] GameObject weapon;
    [SerializeField] List<GameObject> _UIObj = new List<GameObject>();

    [SerializeField] bool _hasWeapon = false;
    public bool HasWeapon { get { return _hasWeapon; } set { _hasWeapon = value; } }


    [SerializeField] GameObject _weaponCursor;
    [SerializeField] GameObject _menuCursor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //turns this into a singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Instance = null;
            Instance = this;
        }

        Cursor.visible = false;

        if (_hasWeapon)
        {
            _weaponCursor.SetActive(true);
        }

    }

    private void OnEnable()
    {
        Inventory.onHoldItem += disableAttack;
    }

    private void OnDisable()
    {
        Inventory.onHoldItem -= disableAttack;
    }

    void setCanMove(bool canMove)
    {
        FindFirstObjectByType<PlayerMovement>().CanMove = canMove;
    }

    // called by scripts + delegates to update weapon state
    public void disableAttack(bool canAttack)
    {
        if (_hasWeapon)
        {
            weapon.SetActive(!canAttack);
            _weaponCursor.SetActive(!canAttack);
        }
    }

    public void disableShopCursor(bool canShopCursor)
    {
        _menuCursor.SetActive(canShopCursor);
    }

    // disables the UI
    void disableUI(bool enable)
    {
        foreach(var UI in _UIObj)
        {
            UI.SetActive(!enable);
        }
    }



    private void Update()
    {
        if (_hasWeapon && _weaponCursor.activeSelf)
        {
            _weaponCursor.transform.position = Input.mousePosition;
        }

        if (_menuCursor.activeSelf)
        {
            _menuCursor.transform.position = Input.mousePosition;
        }
    }

}
