using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Actor, IInteractableActor
{
    [SerializeField] Door ControlledDoor;
    public bool IsPressed = false;
    public bool CanInteract => throw new System.NotImplementedException();

    public void Interact(Actor actor, IInteractableActor.InteractState state, Vector2 movingDir)
    {
        if(actor.TryGetComponent(out MovableActor movableActor) && movingDir != Vector2.zero)
        {
            if(state == IInteractableActor.InteractState.Enter) //Enter the same tile, trigger pressure plate
            {
                IsPressed = true;
                ControlledDoor.OpenDoor();
            }
            else if(state == IInteractableActor.InteractState.Leave) //Leave the same tile, stop triggering pressure plate
            {
                IsPressed = false;
                ControlledDoor.CloseDoor();
            }
        }
    }
}
