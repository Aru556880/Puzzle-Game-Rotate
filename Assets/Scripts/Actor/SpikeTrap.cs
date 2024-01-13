using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : Actor, IInteractableActor
{
    public void Interact(Actor actor)
    {
        print(actor);
        
    }
}
