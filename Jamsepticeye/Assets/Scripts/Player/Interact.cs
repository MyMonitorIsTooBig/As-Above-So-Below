using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{
    InputAction _interactAction;
    InputAction _moveAction;
    PlayerMovement _movement;
    bool _canInteract = true;

    [SerializeField]
    GameObject closestInt;

    [SerializeField]
    float interactRadius = 10f;

    
    Collider2D[] nearbyObjects;

    [SerializeField]
    List<Collider2D> nearbyInteracts = new List<Collider2D>();

    [SerializeField] Inventory _inventory;

    [SerializeField] Transform _interactPoint;

    [SerializeField] GameObject _interactDebugObject;
    [SerializeField] GameObject _interactButtonPrompt;

    bool _moving = false;

    [SerializeField] LayerMask _layerMask;

    public delegate void OnInteract();
    public static OnInteract onInteract;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
        _moveAction = InputSystem.actions.FindAction("Move");
        _movement = GetComponent<PlayerMovement>();

        _interactAction.started += interact;
        _interactAction.performed += holdInteract;
        _moveAction.canceled += OnMoveCanceled;
        _moveAction.started += OnMoveStarted;

    }

    private void OnDisable()
    {
        _interactAction.started -= interact;
        _interactAction.performed -= holdInteract;
        _moveAction.canceled -= OnMoveCanceled;
        _moveAction.started -= OnMoveStarted;

    }

    private void Update()
    {
        // no input action delegate for when the action is being held so I had to do this unfortunately
        if (_moving)
        {
            _interactDebugObject.transform.position = transform.position + ((Vector3)_moveAction.ReadValue<Vector2>() / 2);
        }

        // probably going to change this
        if (_canInteract && _interactPoint != null)
        {

            nearbyObjects = Physics2D.OverlapCircleAll(_interactPoint.position, interactRadius, _layerMask);
            nearbyInteracts = nearbyObjects.ToList();

            //convert array to list so that we can dynamically delete objects that we don't want
            for (int i = 0; i < nearbyInteracts.Count; i++)
            {
                if (nearbyInteracts[i].GetComponent<IInteractable>() == null)
                {
                    nearbyInteracts.RemoveAt(i);
                    i--;
                    continue;
                }
                if (!nearbyInteracts[i].GetComponent<IInteractable>().getInteractState())
                {
                    nearbyInteracts.RemoveAt(i);
                    i--;
                }
            }


            //checks if the closest object is no longer interactable
            if (closestInt != null && !closestInt.GetComponent<IInteractable>().getInteractState())
            {
                closestInt.GetComponent<IInteractable>().cannotInteract();
                closestInt = null;
            }

            //checks if there's a closest object but no nearby objects anymore
            if (nearbyInteracts.Count <= 0)
            {
                if (closestInt != null)
                {
                    closestInt.GetComponent<IInteractable>().cannotInteract();
                    closestInt = null;
                }
            }


            //check which object in the list is the closest then update the states
            if (nearbyInteracts.Count > 0)
            {
                //_interactButtonPrompt.SetActive(true);
                foreach (var _object in nearbyInteracts)
                {
                    if (_object.GetComponent<IInteractable>() != null)
                    {
                        IInteractable IO = _object.GetComponent<IInteractable>();

                        float distance = Vector3.Distance(_interactPoint.position, _object.transform.position);

                        if (closestInt == null)
                        {
                            if (IO.getInteractState())
                            {
                                closestInt = _object.gameObject;
                                closestInt.GetComponent<IInteractable>().canInteract();
                            }
                            return;
                        }

                        if (distance < Vector3.Distance(_interactPoint.position, closestInt.transform.position) && IO.getInteractState())
                        {
                            closestInt.GetComponent<IInteractable>().cannotInteract();
                            closestInt = _object.gameObject;
                            closestInt.GetComponent<IInteractable>().canInteract();
                        }
                    }
                }
            }
            else
            {
                _interactButtonPrompt.SetActive(false);
            }


        }


    }

    // called on the frame the interact action is pressed, checks closest interactable object and initates interact 
    void interact(InputAction.CallbackContext context) 
    {

        if (closestInt != null && _canInteract)
        {

            if(_inventory.heldItem != null)
            {
                _inventory.heldItem.interaction(closestInt.GetComponent<IInteractable>());
            }
            else
            {
                closestInt.GetComponent<IInteractable>().interact(_inventory.heldItem);
            }
        }
    }

    // called when the interact action has been held for a second, checks closest interactable object and initates the hold interact method
    void holdInteract(InputAction.CallbackContext context)
    {
        

        if (closestInt != null && _canInteract)
        {

            if (_inventory.heldItem != null)
            {
                _inventory.heldItem.interaction(closestInt.GetComponent<IInteractable>());
            }
            else
            {
                closestInt.GetComponent<IInteractable>().holdInteract(_inventory.heldItem);
            }
        }
    }


    // called when the move action has been released, sets boolean to false
    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moving = false;
    }

    
    // called when the move action has been started, sets boolean to true
    void OnMoveStarted(InputAction.CallbackContext context)
    {
        _moving = true;
    }

    // called when interaction needs to be disabled, set to delegates such as OnEnterShop 
    public void disableInteract(bool newbool)
    {
        _canInteract = !newbool;
        if (!_canInteract)
        {
            if (closestInt != null) closestInt.GetComponent<IInteractable>().cannotInteract();
        }
        else
        {
            if (closestInt != null) closestInt.GetComponent<IInteractable>().canInteract();
        }
    }

}
