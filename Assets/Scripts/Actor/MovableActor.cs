using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovableActor : Actor
{
    protected bool _isFalling = false;
    #region VIRTUAL_METHOD
    public virtual bool WillFallDown()
    {
        Vector2 floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));
        return !IsOccupied(floorPos);
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
            if(actor.TryGetComponent(out Box box)) //Maybe use interface here
            {
                Coroutine coroutine = box.StartCoroutine(box.MovingBoxCoroutine(movingDir));
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        return activedCoroutine;
    }
    protected List<Coroutine> FallingActorsCoroutines()
    {
        List<GameObject> fallingActors = GetFallingActor();
        List<Coroutine> activedCoroutine = new ();

        foreach(var actor in fallingActors)
        {
            if(actor.TryGetComponent(out Box box)) //Maybe use interface here
            {
                Coroutine coroutine = box.StartCoroutine(box.FallDownAnimation());
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
            else if(actor.TryGetComponent(out Player player))
            {
                Coroutine coroutine = player.StartCoroutine(player.FallDownAnimation());
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        return activedCoroutine;
    }
    #endregion 

    #region GET_SOMETHING_FOR_ROTATING
    Vector2 GetRotatePivot(Vector2 contactWallCenter, Vector2 movingDir)
    {
        return contactWallCenter + _gridSize * 0.5f * (movingDir - GetContactDir(movingDir));
    }
    int GetRotateDir(Vector2 movingDir)
    {
        //1: counterclockwise, -1: clockwise
        Vector2 contactDir = GetContactDir(movingDir);
        if(contactDir == Util.ClockwiseNextDir(movingDir))
            return -1;
        else if(contactDir == Util.ClockwisePrevDir(movingDir))
            return 1;

        return 0;
    }
    Vector2 GetContactDir(Vector2 movingDir)
    {
        Vector2 direction1 = Util.ClockwiseNextDir(movingDir);
        Vector2 direction2 = Util.ClockwisePrevDir(movingDir);
        Vector2 position1 = Util.GetCertainPosition(transform.position,direction1); 
        Vector2 position2 = Util.GetCertainPosition(transform.position,direction2); 

        if( IsOccupied(position1) && IsOccupied(position2) )
        {
            //Debug.Log("The square is between walls!");
            return Vector2.zero;
        }
        else if( IsOccupied(position1) && !IsOccupied(position2) )
        {
            return direction1;
        }
        else if( !IsOccupied(position1) && IsOccupied(position2) )
        {
            return direction2;
        }

        Debug.Log("Cannot get contact direction!");
        return new Vector2(-1,-1);
    }
    #endregion
    
    #region CHECK_MOVING
    bool CanPlayerMove(Vector2 movingDir, Vector2 contactWallPos)
    {
        Vector2 currentPos = transform.position;
        Vector2 nextPos = Util.GetCertainPosition(currentPos, movingDir);

        if(movingDir.x == 0 && movingDir.y == 0) return false;
        else if(!IsOccupied(contactWallPos)) return false;
        else if(IsWall(nextPos)) return false;


        List<GameObject> occupyingActors = GetActorsAtPos(nextPos);
        foreach(var actor in occupyingActors)
        {
            //Maybe use interface method here
            if(actor.TryGetComponent(out Box box))
            {
                if(!box.CanBoxMove(movingDir)) return false;
            }
        }

        return true;
    }
    bool IsWalkableWall(Vector2 position)
    {
        TileBase tile = GameManager.Instance.levelBuilder.GetTileAt(position);
        if(tile==null) return false;
        
        return true;
    }
    bool CanRotate(Vector2 movingDir) //Can rotate if : not between walls at current position and next position
    {
        Vector2 nextPos = Util.GetCertainPosition(transform.position, movingDir);
        Vector2 direction1 = Util.ClockwiseNextDir(movingDir);
        Vector2 direction2 = Util.ClockwisePrevDir(movingDir);

        Vector2 currentNormalPos1 = Util.GetCertainPosition(transform.position,direction1); 
        Vector2 currentNormalPos2 = Util.GetCertainPosition(transform.position,direction2); 
        Vector2 nextNormalPos1 = Util.GetCertainPosition(nextPos,direction1); 
        Vector2 nextNormalPos2 = Util.GetCertainPosition(nextPos,direction2); 

        if(IsOccupied(currentNormalPos1)||IsOccupied(nextNormalPos1))
        {
            return !(IsOccupied(currentNormalPos2)||IsOccupied(nextNormalPos2));
        }
        else if(IsOccupied(currentNormalPos2)||IsOccupied(nextNormalPos2))
        {
            return !(IsOccupied(currentNormalPos1)||IsOccupied(nextNormalPos1));
        }

        return false;
    }
    #endregion

    #region COROUTINES
    public IEnumerator FallDownAnimation()
    {
        if(_isFalling) yield break;

        _isFalling = true;
        Vector2 floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));

        while(WillFallDown())
        {
            Vector2 origPos = transform.position;
            float progress = 0;
            float duration = 0.1f;
            while(progress < duration)
            {
                progress = Mathf.Min(duration, progress + Time.deltaTime);
                transform.position = Vector2.Lerp(origPos, floorPos, progress/duration);
                yield return null;
            }

            Centralize();
            floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));
        }

        List<GameObject> fallingActors = new ();
        List<Coroutine> activedCoroutine = new ();
        fallingActors = GetFallingActor();
        foreach(var actor in fallingActors)
        {
            if(actor.TryGetComponent(out Box box)) //Maybe use interface here
            {
                Coroutine coroutine = box.StartCoroutine(box.FallDownAnimation());
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
            else if(actor.TryGetComponent(out Player player))
            {
                Coroutine coroutine = player.StartCoroutine(player.FallDownAnimation());
                coroutine = player.StartCoroutine(player.FallDownAnimation());
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        TriggetInteractableActors();
        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine));
        _isFalling = false;
    }

    public IEnumerator TranslatingAnimation(Vector2 movingDir)
    {
        Vector2 nextPos = Util.GetCertainPosition(transform.position, movingDir);
        Vector2 origPos = transform.position;
        float progress = 0;
        float duration = 0.25f;

        while(progress < duration)
        {
            progress = Mathf.Min(duration, progress + Time.deltaTime);
            transform.position = Vector2.Lerp(origPos, nextPos, progress/duration);
            yield return null;
        }

        Centralize();
        yield return null;
    }
    #endregion

    #region OTHER_METHODS
    protected List<GameObject> GetFallingActor()
    {
        List<GameObject> actorList = new ();
        foreach(Transform child in _actors)
        {
            if(child.TryGetComponent(out MovableActor actor) && actor.WillFallDown())
            {
                if(!actorList.Contains(child.gameObject)) actorList.Add(child.gameObject);
            }
        }

        return actorList;
    }
    protected void TriggetInteractableActors()
    {
        List<GameObject> occupyingActors = GetActorsAtPos(transform.position);
        foreach(GameObject occupyingActor in occupyingActors)
        {
            if(occupyingActor.TryGetComponent(out IInteractableActor interactableActor))
                interactableActor.Interact(this);
        }
    }
    #endregion
}
