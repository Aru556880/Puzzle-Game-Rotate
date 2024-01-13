using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public enum SquareColor
    {
        None,
        Blue,
        Red,
    }
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
    protected bool IsOccupied(Vector2 position) //The position is occupied by the wall or some object
    {
        if(IsWall(position)) return true;
        
        foreach(Transform actor in _actors)
        {
            Vector2 actorGridPos = GetGridPos(actor.transform.position);
            Vector2 targetGridPos = GetGridPos(position);

            if(targetGridPos != actorGridPos) continue;
            if(actor.TryGetComponent(out Box _))
            {
                return true;
            }
            else if(actor.TryGetComponent(out Player _))
            {
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
}
