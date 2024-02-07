using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MovableActor : Actor //Objects on the tilemap that can be pushed
{
    [SerializeField] float movingSpeed = 1;

    #region OVERRIDE
    public override bool IsBlocked(Vector2 movingDir)
    {
        return !CanBePushed(movingDir);
    }
    #endregion

    #region VIRTUAL_METHOD_MOVING_RELATED
    protected virtual bool CanMoveWhenControlled(Vector2 movingDir, Vector2 contactWallPos) //Defult: Only check next position is blocked or not
    {
        Vector2 currentPos = transform.position;
        Vector2 nextPos = Util.GetCertainPosition(currentPos, movingDir);

        if(movingDir.x == 0 && movingDir.y == 0) return false;
        else if(IsWall(nextPos)) return false;


        List<GameObject> occupyingActors = GetActorsAtPos(nextPos);
        foreach(var element in occupyingActors)
        {
            if(element.TryGetComponent(out Actor actor))
            {
                if(actor.IsBlocked(movingDir)) return false;
            }
        }

        return true;
    }
    
    protected virtual bool CanBePushed(Vector2 movingDir)
    {
        Vector2 currentPos = transform.position;
        Vector2 nextPos = Util.GetCertainPosition(currentPos, movingDir);

        if(movingDir.x == 0 && movingDir.y == 0) return false;
        else if(IsWall(nextPos)) return false;

        List<GameObject> occupyingActors = GetActorsAtPos(nextPos);
        foreach(var element in occupyingActors)
        {
            if(element.TryGetComponent(out Actor actor))
            {
                if(actor.IsBlocked(movingDir)) return false;
            }
        }

        return true;
    }
    #endregion 

    #region VIRTUAL_METHOD_INTERACT_RELATED
    protected virtual void InteractaWithActors(Vector2 movingDir)
    {
        List<GameObject> actorListAtCurrentPos = GetActorsAtPos(transform.position);
        foreach(GameObject occupyingActor in actorListAtCurrentPos)
        {
            //Interact with the actor at same position
            if(occupyingActor.TryGetComponent(out IInteractableActor interactableActor))
                interactableActor.Interact(this, IInteractableActor.InteractState.Enter, movingDir);
        }

        List<GameObject> actorListAtPrevPos = GetActorsAtPos((Vector2)transform.position - movingDir);
        foreach(GameObject occupyingActor in actorListAtPrevPos)
        {
            //Interact with the actor at previous position
            if(occupyingActor.TryGetComponent(out IInteractableActor interactableActor))
                interactableActor.Interact(this, IInteractableActor.InteractState.Leave, movingDir);
        }
    }
    #endregion

    #region GET_COROUTINES_FROM_OTHERS
    protected List<Coroutine> PushActorsCoroutines(Vector2 pushedPos, Vector2 movingDir) 
    {
        //return coroutine of all moving pushed boxes
        
        List<GameObject> pushedActors = new ();
        List<Coroutine> activedCoroutine = new ();

        pushedActors = GetActorsAtPos(pushedPos);
        foreach(var actor in pushedActors)
        {
            if(actor.TryGetComponent(out MovableActor movableActor)) //Maybe use interface here
            {
                if(actor.TryGetComponent(out CharacterFree _)) continue;

                Coroutine coroutine = movableActor.StartCoroutine(movableActor.BePushingCoroutine(movingDir));
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        return activedCoroutine;
    }
    #endregion 

    #region COROUTINES
    public IEnumerator TranslatingAnimation(Vector2 movingDir)
    {
        Vector2 nextPos = Util.GetCertainPosition(transform.position, movingDir);
        Vector2 origPos = transform.position;
        float progress = 0;
        float duration = 0.25f;

        while(progress < duration)
        {
            progress = Mathf.Min(duration, progress + Time.deltaTime * movingSpeed);
            transform.position = Vector2.Lerp(origPos, nextPos, progress/duration);
            yield return null;
        }

        Centralize();
        yield return null;
    }
    public IEnumerator BePushingCoroutine(Vector2 movingDir)
    {
        List<Coroutine> activedCoroutine = new ();
        Vector2 nextPos = Util.GetCertainPosition(transform.position, movingDir);

        if(!CanBePushed(movingDir)) yield break;

        activedCoroutine = Util.MergeList(activedCoroutine, PushActorsCoroutines(nextPos, movingDir));

        yield return StartCoroutine(TranslatingAnimation(movingDir));

        InteractaWithActors(movingDir);

       // activedCoroutine = Util.MergeList(activedCoroutine, FallingActorsCoroutines());

        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine));

        yield return null;
    }
    #endregion

}
