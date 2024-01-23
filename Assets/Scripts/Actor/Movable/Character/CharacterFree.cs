using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFree : MovableActor
{
    public void TryPossess()
    {
        MovableActor possessedActor = FindPossessTarget();

        if(possessedActor!=null)
        {
            possessedActor.BePossessed(this);
        }
    }
    MovableActor FindPossessTarget()
    {
        foreach(GameObject actor in GetActorsAtPos(transform.position))
        {
            if(actor.TryGetComponent(out MovableActor movableActor) && movableActor.CanBePossessed)
            {
                return movableActor;
            }
        }

        return null;
    }
    void ShowPossessTargetHint(Vector2 position)
    {

    }
    #region OVERRIDE
    public override bool CanBePossessed => false;
    protected override bool CanMoveWhenControlled(Vector2 movingDir, Vector2 contactWallPos)
    {
        return true;
    }
    #endregion
}
