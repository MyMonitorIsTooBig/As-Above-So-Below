using UnityEngine;

public interface IInteractable
{
    void interact(Item possibleItem);

    public virtual void holdInteract(Item possibleItem) { }

    void canInteract();
    void cannotInteract();

    bool getInteractState();

}
