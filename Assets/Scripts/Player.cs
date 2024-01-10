using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Vector2Int HeadDirection;
    public bool CanPlayerControl;
    float gridSize { get { return GameManager.Instance.levelBuilder.GridSize;} }
    void Start()
    {
        HeadDirection = new Vector2Int(1,0);
        CanPlayerControl = true;
    }


    void Update()
    {
        
    }
    void Centralize()
    {
        Vector2Int gridPos = GameManager.Instance.levelBuilder.GetGridFromWorld(transform.position);
        transform.position = GameManager.Instance.levelBuilder.GetWorldFromGrid(gridPos);
    }
    #region MOVING_RELATED
    public void Move(Vector2 direction)
    {
        if(Mathf.Abs(direction.x) < 0.5f)
        {
            direction.x = 0;
        }
        else 
        {
            direction.y = 0;
        }

        direction.Normalize();
        Vector2 nextPosition = Util.GetCertainPosition(transform.position, direction);
        Vector2 contactWallCenter = Util.GetCertainPosition(transform.position, Util.ClockwiseNextDir(direction));
        Vector2 rotatePivot = GetRotatePivot(contactWallCenter, direction);
        
        if(!IsBlocked(transform.position, direction, contactWallCenter))
        {
            test.transform.position = rotatePivot;
            StartCoroutine(RotateAnimation(direction, rotatePivot));
            
        }
    }
    Vector2 GetRotatePivot(Vector2 contactWallCenter, Vector2 movingDir)
    {
        return contactWallCenter + (movingDir + Util.ClockwisePrevDir(movingDir)) * gridSize * 0.5f;
    }
    bool IsWall(Vector2 position)
    {
        return GameManager.Instance.levelBuilder.IsWall(position);
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
    IEnumerator RotateAnimation(Vector2 movingDir, Vector2 rotatePivot)
    {
        CanPlayerControl = false;
        
        Vector3 prevRotate = transform.rotation.eulerAngles;
        float progress = 0;
        float duration = 0.25f;

        while(progress < duration)
        {
            float rotateAngle = - Time.deltaTime * 90 / duration;

            if(progress + Time.deltaTime > 1)
            {
                rotateAngle =  - (duration - progress) * 90 / duration;
            }

            progress += Time.deltaTime;
            transform.RotateAround(rotatePivot, new Vector3(0,0,1) , rotateAngle);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(prevRotate + new Vector3(0,0,-90));
        Centralize();
        CanPlayerControl = true;
        yield return null;
    }
    #endregion 
    [SerializeField] GameObject test;
}
