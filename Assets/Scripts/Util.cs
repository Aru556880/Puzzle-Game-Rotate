using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour
{
    //Directions are (1,0), (0,1), (-1,0), (0,-1)
    public static Vector2 ClockwiseNextDir(Vector2 direction)
    {
        return new Vector2(direction.y, -direction.x);
    }
    public static Vector2 ClockwisePrevDir(Vector2 direction)
    {
        return new Vector2(-direction.y, direction.x);
    }
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
