using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDoor : Actor, IInteractableActor
{
    public Util.CardinalDirection KeyDirection;
    public bool HasKey;
    public bool CanInteract { get { return HasKey; }}

    public void Interact(Actor actor, IInteractableActor.InteractState state, Vector2 movingDir)
    {
        Vector2 keyOpposDirVec = Util.GetVecDirFromCardinalDir( Util.GetOppositeDir(KeyDirection));
        
        if(actor.TryGetComponent(out Cage cage))
        {
            Vector2 lockDir = Util.GetVecDirFromCardinalDir(cage.LockDirection);
            if(lockDir == keyOpposDirVec && state == IInteractableActor.InteractState.Enter)
            {
                cage.UnLock();
                HasKey = false;

                gameObject.SetActive(false); //This may replace by animation of opening door
            }

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
