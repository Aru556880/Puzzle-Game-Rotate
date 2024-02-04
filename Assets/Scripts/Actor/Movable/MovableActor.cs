using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//Currently MovableActor is assumed to be the only thing that can be possessed,
//If there are others thing can also be possessed, we need to use Interface instead
public abstract class MovableActor : Actor //Objects on the tilemap that can move (be pushed/fall down aand so on)
{
    protected bool _isFalling = false;
    [SerializeField] float movingSpeed = 1;

    #region OVERRIDE
    public override bool IsBlocked(Vector2 movingDir)
    {
        return !CanBePushed(movingDir);
    }
    #endregion

    //Subclasses must implement their abstract functions
    #region ABSTRACT_METHODS
    public abstract IEnumerator MovedByPlayerCoroutine(Vector2 direction);

    #endregion

    #region VIRTUAL_METHOD_POSSESS_RELATED
    public virtual bool IsPossessed(out CharacterFree possessingChar) //This block is possessed if CharacterFree is set to be its child
    {
        foreach(Transform child in transform)
        {
            if(child.TryGetComponent(out CharacterFree charFree))
            {
                possessingChar = charFree;
                return true;
            }
        }
        
        possessingChar = null;
        return false;
    }
    public virtual bool CanBePossessed{ get{ return !IsPossessed(out _); }}
    public virtual void BePossessed(CharacterFree possessingChar) //Now we control this movable actor
    {
        if(!IsPossessed(out _)) 
        {
            possessingChar.transform.SetParent(transform);
            possessingChar.gameObject.SetActive(false);
            Player.Instance.CurrentControlActor = gameObject;
        }
    }
    public virtual void StopPossessing() //Leave the possessed movable actor
    {   
        if(IsPossessed(out CharacterFree possessingChar))
        {
            possessingChar.gameObject.SetActive(true);
            possessingChar.transform.SetParent(_actorsTransform);
            possessingChar.transform.position = transform.position;  //The free character is spawned at this block
            Player.Instance.CurrentControlActor = possessingChar.gameObject;
        }
    }
    #endregion

    #region VIRTUAL_METHOD_MOVING_RELATED
    protected virtual bool WillFallDown() //Check if this block will fall down 
    {
        Vector2 floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));

        return !IsOccupiedAt(floorPos); //Defult: Check it is on the ground or not
    }
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
    protected List<Coroutine> FallingActorsCoroutines()
    {
        List<GameObject> fallingActors = GetWillFallDownActors();
        List<Coroutine> activedCoroutine = new ();

        foreach(var actor in fallingActors)
        {
            if(actor.TryGetComponent(out MovableActor movableActor))
            {
                Coroutine coroutine = movableActor.StartCoroutine(movableActor.FallDownAnimation());
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        return activedCoroutine;
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
        fallingActors = GetWillFallDownActors();
        foreach(var actor in fallingActors)
        {
            if(actor.TryGetComponent(out MovableActor movableActor)) 
            {
                Coroutine coroutine = movableActor.StartCoroutine(movableActor.FallDownAnimation());
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        InteractaWithActors(new Vector2(0,-1));
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

        activedCoroutine = Util.MergeList(activedCoroutine, FallingActorsCoroutines());

        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine));

        yield return null;
    }
    #endregion

    #region OTHER_METHODS
    protected List<GameObject> GetWillFallDownActors()
    {
        List<GameObject> actorList = new ();
        foreach(Transform child in _actorsTransform)
        {
            if(child.TryGetComponent(out MovableActor actor) && actor.WillFallDown())
            {
                if(!actorList.Contains(child.gameObject)) actorList.Add(child.gameObject);
            }
        }

        return actorList;
    }
    #endregion
}
