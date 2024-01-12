using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour
{
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
}
