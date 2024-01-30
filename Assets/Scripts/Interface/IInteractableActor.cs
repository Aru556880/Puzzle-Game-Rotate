using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableActor
{
    public enum InteractState{Enter, Leave};
    public bool CanInteract { get; }

    //According the direction parameter, this interactable will be trigger.
    //For example if the object is trigger at the same tile of caller, then direction = (0,0)
    public void Interact(Actor actor, InteractState state, Vector2 movingDir);
}
