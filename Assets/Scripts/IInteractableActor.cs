using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableActor
{
    public bool CanInteract { get; }

    public void Interact(Actor actor);
}
