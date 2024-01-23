using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//Currently MovableActor is assumed to be the only thing that can be possessed,
//If there are others thing can also be possessed, we need to use Interface instead
public class MovableActor : Actor //Objects on the tilemap that can move (be pushed/rotate/fall down aand so on)
{
    protected bool _isFalling = false;

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
    public virtual void BePossessed(CharacterFree possessingChar)
    {
        if(!IsPossessed(out _)) 
        {
            possessingChar.transform.SetParent(transform);
            possessingChar.gameObject.SetActive(false);
            Player.Instance.CurrentControlActor = gameObject;
        }
    }
    public virtual void StopPossessing()
    {   
        if(IsPossessed(out CharacterFree possessingChar))
        {
            possessingChar.gameObject.SetActive(true);
            possessingChar.transform.SetParent(GameManager.Instance.levelBuilder.AllActors.transform);
            possessingChar.transform.position = transform.position;  //The free character is spawned at this block
            Player.Instance.CurrentControlActor = possessingChar.gameObject;
        }
    }
    #endregion

    #region VIRTUAL_METHOD_MOVING_RELATED
    public virtual bool WillFallDown() //Check if this block will fall down for current map
    {
        Vector2 floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));
        Vector2 contactPos1 = Util.GetCertainPosition(transform.position, new Vector2(1,0));
        Vector2 contactPos2 = Util.GetCertainPosition(transform.position, new Vector2(-1,0));
        Vector2 contactPos3 = Util.GetCertainPosition(transform.position, new Vector2(0,1));

        if(Player.Instance.CurrentControlActor == gameObject) //current control actor is this
        {
            return !IsOccupiedAt(floorPos) && !IsWalkableWall(contactPos1) 
            && !IsWalkableWall(contactPos2) && !IsWalkableWall(contactPos3); //can stop at wall
        }

        return !IsOccupiedAt(floorPos);
    }
    protected virtual bool CanMoveWhenControlled(Vector2 movingDir, Vector2 contactWallPos)
    {
        Vector2 currentPos = transform.position;
        Vector2 nextPos = Util.GetCertainPosition(currentPos, movingDir);

        if(movingDir.x == 0 && movingDir.y == 0) return false;
        else if(!IsOccupiedAt(contactWallPos)) return false;
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
    public virtual IEnumerator MovedByPlayerCoroutine(Vector2 direction)
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
        Vector2 abovePos = Util.GetCertainPosition(transform.position, new Vector2(0,1));
        Vector2 contactWallCenter = Util.GetCertainPosition(transform.position, GetContactDir(direction));
        Vector2 rotatePivot = GetRotatePivot(contactWallCenter, direction);
        Vector2 nextPos = Util.GetCertainPosition(transform.position, direction);
        float rotateAngle = GetRotateAngle(direction);
        
        List<GameObject> fallingActors = new ();
        List<Coroutine> activedCoroutine = new ();

        if(!CanMoveWhenControlled(direction, contactWallCenter))
        {
            yield break;
        }

        activedCoroutine = Util.MergeList(activedCoroutine, PushActorsCoroutines(nextPos, direction));

        if(rotateAngle != 0)
        {
            yield return StartCoroutine(RotateAnimation(direction, rotatePivot, rotateAngle));
        }
        else
        {
            yield return StartCoroutine(TranslatingAnimation(direction));
        }

        TriggetInteractableActors();

        activedCoroutine = Util.MergeList(activedCoroutine, FallingActorsCoroutines());

        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine)); //Wait for other coroutines finished

    }
    #endregion 

    #region OVERRIDE
    public override bool IsBlocked(Vector2 movingDir)
    {
        return !CanBePushed(movingDir);
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

    #region GET_SOMETHING_FOR_ROTATING
    Vector2 GetRotatePivot(Vector2 contactWallCenter, Vector2 movingDir)
    {
        return contactWallCenter + _gridSize * 0.5f * (movingDir - GetContactDir(movingDir));
    }
    float GetRotateAngle(Vector2 movingDir)
    {
        float rotateDir = 0;
        Vector2 contactDir = GetContactDir(movingDir);
        Vector2 contactPos = Util.GetCertainPosition(transform.position, contactDir);
        Vector2 nextPos = Util.GetCertainPosition(transform.position, movingDir);
        Vector2 direction1 = Util.ClockwiseNextDir(movingDir);
        Vector2 direction2 = Util.ClockwisePrevDir(movingDir);

        Vector2 currentNormalPos1 = Util.GetCertainPosition(transform.position,direction1); 
        Vector2 currentNormalPos2 = Util.GetCertainPosition(transform.position,direction2); 
        Vector2 nextNormalPos1 = Util.GetCertainPosition(nextPos,direction1); 
        Vector2 nextNormalPos2 = Util.GetCertainPosition(nextPos,direction2); 
        Vector2 flipPos = Util.GetCertainPosition(nextNormalPos1, direction1);

        //rotate 0 degree = translation
        if(!IsOccupiedAt(contactPos))
        {
            print("Hi"); 
            return 0;
        }
        else if(IsOccupiedAt(nextPos))
        {
            List<GameObject> occupyingActors = GetActorsAtPos(nextPos);
            foreach(GameObject element in occupyingActors)
            {
                if(element.TryGetComponent(out Actor actor) && actor.IsBlocked(movingDir))
                {
                    return 0;
                }
            }
        }

        //1: counterclockwise, -1: clockwise
        if(contactDir == Util.ClockwiseNextDir(movingDir))
        {
            rotateDir = -1;
            flipPos = nextNormalPos1;
        }      
        else if(contactDir == Util.ClockwisePrevDir(movingDir))
        {
            rotateDir = 1;
            flipPos = nextNormalPos2;
        }
            

        if((IsOccupiedAt(currentNormalPos1)||IsOccupiedAt(nextNormalPos1)) 
        && (IsOccupiedAt(currentNormalPos2)||IsOccupiedAt(nextNormalPos2)) )
        {
            return 0;
        }
        else if(!IsOccupiedAt(flipPos) && Mathf.Abs(movingDir.y) == 1 )
        {
            return rotateDir * 180;  
        }
        else return rotateDir * 90;  

    }
    Vector2 GetContactDir(Vector2 movingDir)
    {
        //For example, if this block is on the ground, then contact direction is down(0,-1)

        Vector2 direction1 = Util.ClockwiseNextDir(movingDir);
        Vector2 direction2 = Util.ClockwisePrevDir(movingDir);
        Vector2 position1 = Util.GetCertainPosition(transform.position,direction1);
        Vector2 position2 = Util.GetCertainPosition(transform.position,direction2);

        if( IsOccupiedAt(position1) && IsOccupiedAt(position2) )
        {
            //Debug.Log("The square is between walls!");
            return Vector2.zero;
        }
        else if( IsOccupiedAt(position1) && !IsOccupiedAt(position2) )
        {
            return direction1;
        }
        else if( !IsOccupiedAt(position1) && IsOccupiedAt(position2) )
        {
            return direction2;
        }

        Debug.Log("Cannot get contact direction!");
        return new Vector2(-1,-1);
    }
    #endregion
    
    #region CHECK_MOVING
    bool CanBePushed(Vector2 movingDir)
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
    bool IsWalkableWall(Vector2 position)
    {
        //Currently all walls can be walked on

        TileBase tile = GameManager.Instance.levelBuilder.GetTileAt(position);
        if(tile==null) return false;

        if(tile.name.Contains("Blue")) return true;

        return false;
    }
    #endregion

    #region COROUTINES
    
    IEnumerator RotateAnimation(Vector2 movingDir, Vector2 rotatePivot, float rotateAngle)
    {
        Vector3 prevRotate = transform.rotation.eulerAngles;
        float progress = 0;
        float duration = 0.25f;

        while(progress < duration)
        {
            float rotateEplison = Time.deltaTime * rotateAngle / duration;

            if(progress + Time.deltaTime > 1)
            {
                rotateEplison =  (duration - progress) * rotateAngle / duration;
            }

            progress += Time.deltaTime;
            transform.RotateAround(rotatePivot, new Vector3(0,0,1) , rotateEplison);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(prevRotate + new Vector3(0,0,rotateAngle));
        Centralize();
        
        yield return null;
    }
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
    public IEnumerator BePushingCoroutine(Vector2 movingDir)
    {
        List<Coroutine> activedCoroutine = new ();
        Vector2 nextPos = Util.GetCertainPosition(transform.position, movingDir);
        Vector2 contactWallCenter = Util.GetCertainPosition(transform.position, GetContactDir(movingDir));

        if(!CanBePushed(movingDir)) yield break;

        activedCoroutine = Util.MergeList(activedCoroutine, PushActorsCoroutines(nextPos, movingDir));

        yield return StartCoroutine(TranslatingAnimation(movingDir));

        TriggetInteractableActors();

        activedCoroutine = Util.MergeList(activedCoroutine, FallingActorsCoroutines());

        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine));

        yield return null;
    }
    #endregion

    #region OTHER_METHODS
    protected List<GameObject> GetWillFallDownActors()
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
    protected virtual void TriggetInteractableActors()
    {
        List<GameObject> occupyingActors = GetActorsAtPos(transform.position);
        foreach(GameObject occupyingActor in occupyingActors)
        {
            if(occupyingActor.TryGetComponent(out IInteractableActor interactableActor))
                interactableActor.Interact(this, Vector2.zero);
        }
    }
    #endregion
}
