using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour
{
    #region DIRECTION
    public enum CardinalDirection
    {
        Up,
        Left,
        Down,
        Right,
    }
    public static Vector2 GetVecDirFromCardinalDir(CardinalDirection cardinalDirection)
    {
        Vector2 direction = Vector2.zero;

        if(cardinalDirection == CardinalDirection.Up)
        {
            direction = new Vector2(0,1);
        }
        else if(cardinalDirection == CardinalDirection.Left)
        {
            direction = new Vector2(-1,0);
        }
        else if(cardinalDirection == CardinalDirection.Down)
        {
            direction = new Vector2(0,-1);
        }
        else if(cardinalDirection == CardinalDirection.Right)
        {
            direction = new Vector2(1,0);
        }

        return direction;
    }
    public static CardinalDirection GetCardinalDirFromVec(Vector2 vectorDirection)
    {
        CardinalDirection direction = CardinalDirection.Up;

        if(vectorDirection == new Vector2(0,1))
        {
            direction = CardinalDirection.Up;
        }
        else if(vectorDirection == new Vector2(-1,0))
        {
            direction = CardinalDirection.Left;
        }
        else if(vectorDirection == new Vector2(0,-1))
        {
            direction = CardinalDirection.Down;
        }
        else if(vectorDirection == new Vector2(1,0))
        {
            direction = CardinalDirection.Right;
        }
        else Debug.Log("No Direction!");

        return direction;
    }

    public static CardinalDirection GetOppositeDir(CardinalDirection direction)
    {
        int dirIndex = ((int)direction + 2) % 4;
        if(dirIndex < 0 || dirIndex > 4 ) Debug.Log("Direction Index Error");
        
        return (CardinalDirection)dirIndex;
    }

    //Directions are (1,0), (0,1), (-1,0), (0,-1)
    public static Vector2 ClockwiseNextDir(Vector2 direction)
    {
        return new Vector2(direction.y, -direction.x);
    }
    public static Vector2 ClockwisePrevDir(Vector2 direction)
    {
        return new Vector2(-direction.y, direction.x);
    }
    #endregion
    public static Vector2 GetCertainPosition(Vector2 point ,Vector2 offset)
    {
        return point + offset * GameManager.Instance.levelBuilder.GridSize;
    }
    public static IEnumerator WaitForCoroutines(List<Coroutine> coroutines)
    {
        for(int i=0;i<coroutines.Count;i++)
        {
            yield return coroutines[i];
        }
        
        yield return null;
    }
    public static List<T> MergeList<T>(List<T> mergedList1, List<T> mergedList2)
    {
        List<T> newList = new ();
        for(int i=0;i<mergedList1.Count;i++)
        {
            T element = mergedList1[i];
            if(!newList.Contains(element)) newList.Add(element); 
        }

        for(int i=0;i<mergedList2.Count;i++)
        {
            T element = mergedList2[i];
            if(!newList.Contains(element)) newList.Add(element); 
        }

        return newList;
    }
}
