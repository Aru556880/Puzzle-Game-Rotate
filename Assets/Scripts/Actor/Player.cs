using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MovableActor
{
    public Vector2Int HeadDirection;
    public bool CanPlayerControl;
    public SquareColor CurrentMode;
    
    SpriteRenderer _spriteRenderer;
    [SerializeField] Sprite _bluePlayerSprite;
    [SerializeField] Sprite _redPlayerSprite;
    void Awake() 
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();    
    }
    void Start()
    {
        HeadDirection = new Vector2Int(1,0);
        CanPlayerControl = true;
        CurrentMode = SquareColor.Blue;
        _spriteRenderer.sprite = _bluePlayerSprite;
    }
    #region OVERRIDE
    public override bool WillFallDown()
    {
        Vector2 floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));
        Vector2 contactPos1 = Util.GetCertainPosition(transform.position, new Vector2(1,0));
        Vector2 contactPos2 = Util.GetCertainPosition(transform.position, new Vector2(-1,0));
        Vector2 contactPos3 = Util.GetCertainPosition(transform.position, new Vector2(0,1));

        return !IsOccupied(floorPos) && !IsWalkableWall(contactPos1) 
        && !IsWalkableWall(contactPos2) && !IsWalkableWall(contactPos3);
    }
    #endregion

    #region PLAYER_RELATED
    public void SwitchMode()
    {
        StartCoroutine(SwitchModeCoroutine());
    }
    #endregion

    #region MOVING_RELATED
    public void Move(Vector2 direction)
    {
        StartCoroutine(MovingCoroutine(direction));
    }
    Vector2 GetRotatePivot(Vector2 contactWallCenter, Vector2 movingDir)
    {
        return contactWallCenter + _gridSize * 0.5f * (movingDir - GetContactDir(movingDir));
    }
    int GetRotateDir()
    {
        //1: counterclockwise, -1: clockwise
        if(CurrentMode == SquareColor.Blue)
            return -1;
        else if(CurrentMode == SquareColor.Red)
            return 1;

        return 0;
    }
    Vector2 GetContactDir(Vector2 movingDir)
    {
        if(CurrentMode == SquareColor.Blue)
            return Util.ClockwiseNextDir(movingDir);
        else if(CurrentMode == SquareColor.Red)
            return Util.ClockwisePrevDir(movingDir);

        return Vector2.zero;
    }
    bool IsWalkableWall(Vector2 position)
    {
        TileBase tile = GameManager.Instance.levelBuilder.GetTileAt(position);
        if(tile==null) return false;
        
        if(CurrentMode == SquareColor.Blue)
            return tile.name.Contains("Blue");
        else if(CurrentMode == SquareColor.Red)
            return tile.name.Contains("Red");
        
        return false;
    }
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
    #endregion

    #region COROUTINE
    IEnumerator SwitchModeCoroutine()
    {
        CanPlayerControl = false;
        if(CurrentMode == SquareColor.Blue)
        {
            CurrentMode = SquareColor.Red;
            _spriteRenderer.sprite = _redPlayerSprite;
        }
        else if(CurrentMode == SquareColor.Red)
        {
            CurrentMode = SquareColor.Blue;
            _spriteRenderer.sprite = _bluePlayerSprite;
        }

        if(!WillFallDown())
        {
            CanPlayerControl = true;
            yield break;
        }

        yield return StartCoroutine(FallDownAnimation());
        CanPlayerControl = true;
    }
    IEnumerator MovingCoroutine(Vector2 direction)
    {
        CanPlayerControl = false;
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
        List<GameObject> pushedActors = new ();
        List<GameObject> fallingActors = new ();
        List<Coroutine> activedCoroutine = new ();

        if(!CanPlayerMove(direction, contactWallCenter))
        {
            CanPlayerControl = true;
            yield break;
        }
        
        pushedActors = GetActorsAtPos(nextPos);
        foreach(var actor in pushedActors)
        {
            if(actor.TryGetComponent(out Box box)) //Maybe use interface here
            {
                Coroutine coroutine = box.StartCoroutine(box.MovingBoxCoroutine(direction));
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        yield return StartCoroutine(RotateAnimation(direction, rotatePivot));

        TriggetInteractableActors();

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
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine));

        CanPlayerControl = true;
        yield return null;
    }
    IEnumerator RotateAnimation(Vector2 movingDir, Vector2 rotatePivot)
    {
        Vector3 prevRotate = transform.rotation.eulerAngles;
        float progress = 0;
        float duration = 0.25f;
        int rotateDir = GetRotateDir();

        while(progress < duration)
        {
            float rotateAngle = rotateDir * Time.deltaTime * 90 / duration;

            if(progress + Time.deltaTime > 1)
            {
                rotateAngle =  rotateDir * (duration - progress) * 90 / duration;
            }

            progress += Time.deltaTime;
            transform.RotateAround(rotatePivot, new Vector3(0,0,1) , rotateAngle);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(prevRotate + new Vector3(0,0,rotateDir*90));
        Centralize();
        yield return null;
    }

    #endregion 
}
