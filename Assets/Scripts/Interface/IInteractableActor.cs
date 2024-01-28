using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableActor
{
    public bool CanInteract { get; }

    //According the direction parameter, this interactable will be trigger.
    //For example if the object is trigger at the same tile of caller, then direction = (0,0)
    public void Interact(Actor actor, Vector2 triggerDir, Vector2 movingDir);
}
