using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    int slotAmt;
    [SerializeField]
    int _selectedSlot = 1;


    


    [SerializeField]
    List<InventorySlot> slots = new List<InventorySlot>();

    [SerializeField]
    List<Item> permaItems = new List<Item>();

    [SerializeField]
    GameObject slotUI;
    [SerializeField]
    Transform slotParent;
    [SerializeField]
    GameObject highlightObj;
    


    InputAction next;
    InputAction prev;
    InputAction holdItem;
    InputAction scrollWheel;
    InputAction dropItem;

    Vector3 offset = new(0,-1f);

    [SerializeField]
    public Item heldItem;

    [SerializeField]
    GameObject heldItemObj;
    [SerializeField]
    SpriteRenderer _heldItemSprite;
    [SerializeField] Animator _heldItemAnim;

    [SerializeField]
    bool canSwapitems = true;
    [SerializeField]bool isHolding = false;

    [SerializeField]bool canHold = true;

    public delegate void OnHoldItem(bool canHold);
    public static OnHoldItem onHoldItem;

    private void OnEnable()
    {
        //sets up inventory and all the different inputs that are used as well as the input events

        createInventory();

        /*
        next = InputSystem.actions.FindAction("Next");
        prev = InputSystem.actions.FindAction("Previous");
        holdItem = InputSystem.actions.FindAction("Jump");
        scrollWheel = InputSystem.actions.FindAction("Scroll");
        dropItem = InputSystem.actions.FindAction("Drop");

        next.performed += prevButton;
        prev.performed += nextButton;
        holdItem.performed += holdItemButton;
        scrollWheel.performed += nextItem;
        dropItem.performed += dropCurrentItem;
        */

        Attack.onAttack += setCanUseInventory;


    }

    //unlinks all the methods from the input events
    private void OnDisable()
    {
        

        Attack.onAttack -= setCanUseInventory;

    }

    
    //clears current inventory then creates set amount of slots
    void createInventory()
    {
        slots.Clear();

        _selectedSlot = slotAmt;

        for(int i = 0; i < slotAmt; i++)
        {
            GameObject sbg = Instantiate(slotUI, slotParent);

            sbg.GetComponent<RectTransform>().anchoredPosition = new Vector3(-i*175, 0, 0);

            slots.Add(new InventorySlot(i + 1, null, sbg));
        }

        if(slots.Count > 0)
        {
            highlightObj.SetActive(true);
            highlightObj.transform.position = slots[_selectedSlot-1]._itemBGUI.transform.position;
        }
        else
        {
            highlightObj.SetActive(false);
        }


    }

    // adds a new slot to players inventory, used to dynamically update amount of slots
    public void addSlot()
    {
        slotAmt++;

        GameObject sbg = Instantiate(slotUI, slotParent);

        sbg.GetComponent<RectTransform>().anchoredPosition = new Vector3(-slots.Count * 175, 0, 0);

        slots.Add(new InventorySlot(slots.Count, null, sbg));
    }



    void setCanSwapItems(bool canSwap, GameObject building) { canSwapitems = !canSwap; }
    
    void setCanSwapItems(bool canSwap) { canSwapitems = !canSwap; }
    void setCanUseInventory(bool canUse) { canHold = canUse; }



    //changes currently selected slot to the next one in the list
    void nextButton(InputAction.CallbackContext context)
    {
        if (canSwapitems)
        {

            if (_selectedSlot != slotAmt) _selectedSlot++;
            else _selectedSlot = 1;

            highlightObj.transform.position = slots[_selectedSlot - 1]._itemBGUI.transform.position;
        }
    }

    //changes currently selected slot to the previous one in the list
    void prevButton(InputAction.CallbackContext context)
    {

        if (canSwapitems)
        {

            if (_selectedSlot != 1) _selectedSlot--;
            else _selectedSlot = slotAmt;

            highlightObj.transform.position = slots[_selectedSlot - 1]._itemBGUI.transform.position;
        }
    }

    //when the player presses the button that makes them hold the currently selected item
    void holdItemButton(InputAction.CallbackContext context)
    {
        if (canSwapitems && !isHolding && canHold)
        {
            if (slots[_selectedSlot - 1]._item != null)
            {
                heldItem = slots[_selectedSlot - 1]._item;
                //heldItemObj.SetActive(true);
                _heldItemSprite.sprite = heldItem.GetComponentInChildren<SpriteRenderer>().sprite;
                
                _heldItemAnim.SetBool("_bringout", true);
                _heldItemAnim.SetBool("_putaway", false);
                //heldItemObj.GetComponent<SpriteRenderer>().sprite = heldItem.GetComponentInChildren<SpriteRenderer>().sprite;

                heldItem.onHold(true);

                onHoldItem?.Invoke(true);

                isHolding = true;
                canSwapitems = false;
            }
        }
        else if(isHolding)
        {

            disableHeldItemObj();
            
        }
    }


    void dropCurrentItem(InputAction.CallbackContext context)
    {
        if (canSwapitems)
        {
            if (slots[_selectedSlot-1]._item != null)
            {
                

                Item go = Instantiate(slots[_selectedSlot - 1]._item, transform.position, Quaternion.identity);
                go.gameObject.SetActive(true);
                go = null;
                removeFromInv(slots[_selectedSlot-1]._item);
            }
        }
    }


    //same thing as prevButton and nextButton but using the scrollwheel instead
    void nextItem(InputAction.CallbackContext context)
    {
        if (canSwapitems)
        {

            if (context.action.ReadValue<Vector2>().y == 1)
            {
                nextButton(context);
            }
            else
            {
                prevButton(context);
            }
        }

    }

    //used when the player is no longer holding an item
    public void disableHeldItemObj()
    {
        if (heldItem != null)
        {
            heldItem.onHold(false);
            
        }

        //heldItemObj.SetActive(false);
       
        _heldItemAnim.SetBool("_bringout", false);
        _heldItemAnim.SetBool("_putaway", true);
        canSwapitems = true;
        isHolding = false;
        heldItem = null;

        

        onHoldItem?.Invoke(false);
    }

    //adds referenced item to an empty inventory slot if there is one
    public void addToInv(Item item)
    {
        //checks every slot for if there is already the item that is being added and adds to that stack
        foreach(InventorySlot _slot in slots)
        {
            if(_slot._item != null)
            {
                if (_slot._item.Stats.Name == item.Stats.Name)
                {
                    _slot._item.gameObject.SetActive(false);
                    _slot._stackAmt++;

                    return;
                }
            }
        }

        //checks for first null inventory slot and adds item to that slot
        foreach (InventorySlot _slot in slots)
        {
            if (_slot._item == null)
            {


                _slot._item = Instantiate(item, gameObject.transform);


                _slot._item.gameObject.SetActive(false);
                _slot._stackAmt++;

                return;
            }
        }

        GameObject _itemSpawn = Instantiate(item.gameObject, transform.position, Quaternion.identity);
    }

    //if the referneced item is found in the inventory then remove it
    public void removeFromInv(Item item)
    {
        foreach (InventorySlot _slot in slots)
        {
            if (_slot._item == item && _slot._item != null)
            {
                _slot._stackAmt--;

                if (_slot._stackAmt <= 0)
                {
                    _slot._stackAmt = 0;

                    Destroy(_slot._item.gameObject);

                    disableHeldItemObj();
                    return;
                }
                
            }
        }
    }

    //adds an item to the player's permanent inventory, used for ability items like "online shopping"
    public void permaInventory(Item item)
    {
        permaItems.Add(Instantiate(item, gameObject.transform));
    }

    //checks if the player's inventory is full
    public bool IsInventoryFull(Item item)
    {
        foreach(InventorySlot _slot in slots)
        {
            if(_slot._item == null || _slot._item.Stats.Name == item.Stats.Name)
            {
                return false;
            }
        }

        return true;
    }

    //custom class for the inventory slots
    [Serializable]
    public class InventorySlot
    {
        int _slotNum;
        public Item _item;
        public GameObject _itemBGUI;
        public int _stackAmt;
        

        public InventorySlot(int slotNum, Item item, GameObject itemBGUI)
        {
            _slotNum = slotNum;
            _item = item;
            _itemBGUI = itemBGUI;
        }

        
    }
}
