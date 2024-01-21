using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDoor : Actor, IInteractableActor
{
    public Util.CardinalDirection KeyDirection;
    public bool HasKey;
    public bool CanInteract { get { return HasKey; }}

    public void Interact(Actor actor)
    {
        print("Key!");
    }

    void Start()
    {
        HasKey = true;
    }
    public override bool IsBlocked(Vector2 movingDir)
    {
        return HasKey;
    }
}
