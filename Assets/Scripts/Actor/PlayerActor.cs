using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerActor : MovableActor
{
    public override IEnumerator ControlActorCoroutine (Vector2 direction)
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
        Vector2 contactWallCenter = Util.GetCertainPosition(transform.position, GetContactDir(direction));
        Vector2 rotatePivot = GetRotatePivot(contactWallCenter, direction);
        Vector2 nextPos = Util.GetCertainPosition(transform.position, direction);
        float rotateAngle = GetRotateAngle(direction);
        
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

        InteractaWithActors(direction);

        //activedCoroutine = Util.MergeList(activedCoroutine, FallingActorsCoroutines());

        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine)); //Wait for other coroutines finished

    }

    #region ROTATE_METHODS
    public void BeginRotateAction(Vector2 movingDir) {} //Do something when rotation starts
    public void PerformRotatingAction(Vector2 movingDir) { }  //Do something when rotationing
    public void EndRotateAction(Vector2 movingDir) {} //Do something when rotation ends
    protected override bool CanMoveWhenControlled(Vector2 movingDir, Vector2 contactWallPos)
    {
        Vector2 currentPos = transform.position;
        Vector2 nextPos = Util.GetCertainPosition(currentPos, movingDir);

        if(movingDir.x == 0 && movingDir.y == 0) return false;
        else if(!IsOccupiedAt(contactWallPos)) return false; //Additional condition: the rotation has available support point
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
        else if(!IsOccupiedAt(flipPos) )
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

    #region COROUTINES
    IEnumerator RotateAnimation(Vector2 movingDir, Vector2 rotatePivot, float rotateAngle)
    {
        BeginRotateAction(movingDir);

        Vector3 prevRotate = transform.rotation.eulerAngles;
        float progress = 0;
        float duration = 0.25f;
        duration = duration * (Mathf.Abs(rotateAngle) / 90f);

        while(progress < duration)
        {
            float rotateEplison = Time.deltaTime * rotateAngle / duration;

            if(progress + Time.deltaTime > 1)
            {
                rotateEplison =  (duration - progress) * rotateAngle / duration;
            }

            progress += Time.deltaTime;
            transform.RotateAround(rotatePivot, new Vector3(0,0,1) , rotateEplison);

            PerformRotatingAction(movingDir);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(prevRotate + new Vector3(0,0,rotateAngle));
        Centralize();
        EndRotateAction(movingDir);

        yield return null;
    }
    #endregion

    bool IsWalkableWall(Vector2 position)
    {
        //Currently all walls can be walked on

        TileBase tile = GameManager.Instance.levelBuilder.GetTileAt(position);
        if(tile==null) return false;

        if(tile.name.Contains("Walkable")) return true;

        return false;
    }
}
