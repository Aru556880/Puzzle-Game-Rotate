using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDoor : Actor, IInteractableActor
{
    public Util.CardinalDirection KeyDirection;
    public bool HasKey;
    public bool CanInteract { get { return HasKey; }}

    public void Interact(Actor actor, Vector2 direction)
    {

        Vector2 keyOpposDirVec = Util.GetVecDirFromCardinalDir( Util.GetOppositeDir(KeyDirection));
        if(direction == keyOpposDirVec && actor.TryGetComponent(out Cage cage))
        {
            cage.UnLock();
            HasKey = false;

            gameObject.SetActive(false); //This may replace by animation of opening door
        }
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
