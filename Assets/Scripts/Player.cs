using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    public enum PlayerSquareMode
    {
        Blue,
        Red,
    }

    public Vector2Int HeadDirection;
    public bool CanPlayerControl;
    public PlayerSquareMode CurrentMode;
    
    float _gridSize { get { return GameManager.Instance.levelBuilder.GridSize;} }
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
        _spriteRenderer.sprite = _bluePlayerSprite;
        CurrentMode = PlayerSquareMode.Blue;
    }
    #region PLAYER_RELATED
    void Centralize()
    {
        Vector2Int gridPos = GameManager.Instance.levelBuilder.GetGridFromWorld(transform.position);
        transform.position = GameManager.Instance.levelBuilder.GetWorldFromGrid(gridPos);
    }
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
        if(CurrentMode == PlayerSquareMode.Blue)
            return -1;
        else if(CurrentMode == PlayerSquareMode.Red)
            return 1;

        return 0;
    }
    Vector2 GetContactDir(Vector2 movingDir)
    {
        if(CurrentMode == PlayerSquareMode.Blue)
            return Util.ClockwiseNextDir(movingDir);
        else if(CurrentMode == PlayerSquareMode.Red)
            return Util.ClockwisePrevDir(movingDir);

        return Vector2.zero;
    }
    bool IsWall(Vector2 position)
    {
        return GameManager.Instance.levelBuilder.IsWall(position);
    }
    bool IsWalkableWall(Vector2 position)
    {
        TileBase tile = GameManager.Instance.levelBuilder.GetTileAt(position);
        if(tile==null) return false;
        
        if(CurrentMode == PlayerSquareMode.Blue)
            return tile.name.Contains("Blue");
        else if(CurrentMode == PlayerSquareMode.Red)
            return tile.name.Contains("Red");
        
        return false;
    }
    bool IsBlocked(Vector2 currentPos, Vector2 movingDir, Vector2 contactWallPos)
    {
        Vector2 nextPos = Util.GetCertainPosition(currentPos, movingDir);

        if(movingDir.x == 0 && movingDir.y == 0) return true;
        else if(!IsWall(contactWallPos)) return true;
        else if(IsWall(nextPos)) return true;

        return false;
    }
    #endregion

    #region COROUTINE
    IEnumerator SwitchModeCoroutine()
    {
        CanPlayerControl = false;
        if(CurrentMode == PlayerSquareMode.Blue)
        {
            CurrentMode = PlayerSquareMode.Red;
            _spriteRenderer.sprite = _redPlayerSprite;
        }
        else if(CurrentMode == PlayerSquareMode.Red)
        {
            CurrentMode = PlayerSquareMode.Blue;
            _spriteRenderer.sprite = _bluePlayerSprite;
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
        Vector2 contactWallCenter = Util.GetCertainPosition(transform.position, GetContactDir(direction));
        Vector2 rotatePivot = GetRotatePivot(contactWallCenter, direction);
        
        if(!IsBlocked(transform.position, direction, contactWallCenter))
        {
            yield return StartCoroutine(RotateAnimation(direction, rotatePivot));
        }

        yield return StartCoroutine(FallDownAnimation());

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
    IEnumerator FallDownAnimation()
    {
        Vector2 floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));
        Vector2 contactPos1 = Util.GetCertainPosition(transform.position, new Vector2(1,0));
        Vector2 contactPos2 = Util.GetCertainPosition(transform.position, new Vector2(-1,0));
        Vector2 contactPos3 = Util.GetCertainPosition(transform.position, new Vector2(0,1));

        while(!IsWall(floorPos) && !IsWalkableWall(contactPos1) 
        && !IsWalkableWall(contactPos2) && !IsWalkableWall(contactPos3))
        {
            Vector2 origPos = transform.position;
            float progress = 0;
            float duration = 0.25f;
            while(progress < duration)
            {
                progress = Mathf.Min(duration, progress + Time.deltaTime);
                transform.position = Vector2.Lerp(origPos, floorPos, progress/duration);
                yield return null;
            }

            Centralize();
            floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));
            contactPos1 = Util.GetCertainPosition(transform.position, new Vector2(1,0));
            contactPos2 = Util.GetCertainPosition(transform.position, new Vector2(-1,0));
        }

        yield return null;
    }
    #endregion 
}
