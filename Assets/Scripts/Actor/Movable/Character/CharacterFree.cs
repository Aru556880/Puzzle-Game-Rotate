using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFree : MovableActor
{
     void OnEnable() 
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
    }
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

    #region IMPLEMENT_ABSTRACT_METHODS
    public override IEnumerator MovedByPlayerCoroutine(Vector2 direction)
    {
        //When player control this block and input WASD, this coroutine will be called

        if(Mathf.Abs(direction.x) < 0.5f)
        {
            direction.x = 0;
        }
        else 
        {
            direction.y = 0;
        }

        direction.Normalize();
        List<Coroutine> activedCoroutine = new ();

        if(direction.x!=0) transform.localScale = new Vector2(direction.x,1);

        if(!CanMoveWhenControlled(direction, Vector2.zero))
        {
            yield break;
        }
        
        yield return StartCoroutine(TranslatingAnimation(direction));

        //TriggerInteractableActors();

        activedCoroutine = Util.MergeList(activedCoroutine, FallingActorsCoroutines());

        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine)); //Wait for other coroutines finished

    }
    #endregion

    #region OVERRIDE
    public override bool CanBePossessed => false;
    public override bool IsBlocked(Vector2 movingDir) { return false; } //Free character is incorporeal
    protected override bool WillFallDown()
    {
        return false;
    }
    protected override bool CanMoveWhenControlled(Vector2 movingDir, Vector2 contactWallPos)
    {
        Vector2 currentPos = transform.position;
        Vector2 nextPos = Util.GetCertainPosition(currentPos, movingDir);

        if(movingDir.x == 0 && movingDir.y == 0) return false;
        else if(IsWall(nextPos)) return false;

        return true;
    }
    #endregion
}
