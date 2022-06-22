using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IDWInteractable : MonoBehaviour
{
    public IDWInteractable Item;
    public virtual void Interact(IDWInteractAgent agent)
    {        
        Item.Interact(agent);
    }
}

public enum DWInteractableState
{
    Start,
    Done
}
public interface IDWInteractAgent
{
    void InteractCallback(DWInteractableState state, IDWInteractable item); 
}