using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2Int HeadDirection;
    // Start is called before the first frame update
    void Start()
    {
        HeadDirection = new Vector2Int(1,0);
    }

    // Update is called once per frame
    void Update()
    {
        
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
        Vector2 nextPosition = GetTargetPosition(transform.position, direction);
        
        if(!IsBlocked(transform.position, direction, out Vector2 contactPos))
        {
            transform.position = nextPosition;
            print(contactPos);
            test.transform.position = contactPos;
        }
    }
    Vector2 GetTargetPosition(Vector2 point ,Vector2 offset)
    {
        return point + offset * GameManager.Instance.levelBuilder.GridSize;
    }
    bool IsWall(Vector2 position)
    {
        return GameManager.Instance.levelBuilder.IsWall(position);
    }
    bool IsBlocked(Vector2 currentPos, Vector2 movingDir, out Vector2 contactPos)
    {
        contactPos = Vector2.zero;
        Vector2 checkedPos1_1;
        Vector2 checkedPos1_2;
        Vector2 checkedPos2_1;
        Vector2 checkedPos2_2;
        Vector2 nextPosition = GetTargetPosition(currentPos, movingDir);

        if(IsWall(nextPosition)) return true;

        if(movingDir.x == 0 && movingDir.y != 0)
        {
            checkedPos1_1 = GetTargetPosition(currentPos, new Vector2(-1,0));
            checkedPos2_1 = GetTargetPosition(currentPos, new Vector2(1,0));
        }
        else if(movingDir.x != 0 && movingDir.y == 0)
        {
            checkedPos1_1 = GetTargetPosition(currentPos, new Vector2(0,-1));
            checkedPos2_1 = GetTargetPosition(currentPos, new Vector2(0,1));
        }
        else
        {
            return true;
        }

        checkedPos1_2 = GetTargetPosition(checkedPos1_1, movingDir);
        checkedPos2_2 = GetTargetPosition(checkedPos2_1, movingDir);

        Vector2[] checkPosArray = 
        {
            checkedPos1_1,
            checkedPos2_1,
        };

        foreach(var checkpos in checkPosArray)
        {
            if(IsWall(checkpos))
            {
                contactPos = checkpos;
                break;
            }
        }

        return (IsWall(checkedPos1_1) || IsWall(checkedPos1_2))
        && (IsWall(checkedPos2_1) || IsWall(checkedPos2_2));
    }
    #endregion
    [SerializeField] GameObject test;
}
