using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour //Any Object on the tile map
{
    protected float _gridSize { get { return GameManager.Instance.levelBuilder.GridSize;} }
    protected Transform _actors { get { return transform==null ? transform : transform.parent; } }
    protected void Centralize()
    {
        Vector2Int gridPos = GameManager.Instance.levelBuilder.GetGridFromWorld(transform.position);
        transform.position = GameManager.Instance.levelBuilder.GetWorldFromGrid(gridPos);
    }
    protected Vector2Int GetGridPos(Vector2 worldPos)
    {
        return GameManager.Instance.levelBuilder.GetGridFromWorld(worldPos);
    }
    protected bool IsWall(Vector2 position)
    {
        return GameManager.Instance.levelBuilder.IsWall(position);
    }
    protected bool IsOccupiedAt(Vector2 position) //The position is occupied by the wall or some object
    {
        if(IsWall(position)) return true;
        
        foreach(Transform actor in _actors)
        {
            Vector2 actorGridPos = GetGridPos(actor.transform.position);
            Vector2 targetGridPos = GetGridPos(position);

            if(targetGridPos != actorGridPos || !actor.gameObject.activeSelf) continue;
            if(actor.TryGetComponent(out MovableActor _))
            {
                if(actor.TryGetComponent(out CharacterFree _)) continue;
                
                return true;
            }

        }

        return false;
    }
    protected List<GameObject> GetActorsAtPos(Vector2 position)
    {
        List<GameObject> actorsList = new ();

        foreach(Transform child in _actors)
        {
            Vector2Int actorGridPos = GetGridPos(child.position);
            Vector2Int targetGridPos = GetGridPos(position);

            if(targetGridPos == actorGridPos && !actorsList.Contains(child.gameObject)) 
                actorsList.Add(child.gameObject);
        }

        return actorsList;
    }
    public Util.CardinalDirection GetInitCertainDirection(Util.CardinalDirection initDir)
    {
        //After rotation, the initial certain direction(up,left,down or right) of this block becomes which direction
       
        Util.CardinalDirection currentDir = Util.CardinalDirection.Up;
        int rotation = (int)transform.rotation.eulerAngles.z / 90 + 4 + (int)initDir;

        if(rotation % 4 == 0)
        {
            currentDir = Util.CardinalDirection.Up;
        }
        else if(rotation % 4 == 1)
        {
            currentDir = Util.CardinalDirection.Left;
        }
        else if(rotation % 4 == 2)
        {
            currentDir = Util.CardinalDirection.Down;
        }
        else if(rotation % 4 == 3)
        {
            currentDir = Util.CardinalDirection.Right;
        }

        return currentDir;
    }
    public virtual bool IsBlocked(Vector2 movingDir){ return false;}
    private void OnDisable() 
    {
        StopAllCoroutines();
    }
}
