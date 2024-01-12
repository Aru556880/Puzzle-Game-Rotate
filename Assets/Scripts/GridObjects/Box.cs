using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Actor
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public bool CanBoxMove(Vector2 movingDir, out List<Actor> activedActors)
    {
        activedActors = new ();
        Vector2 currentPos = transform.position;
        Vector2 nextPos = Util.GetCertainPosition(currentPos, movingDir);

        if(movingDir.x == 0 && movingDir.y == 0) return false;
        else if(IsWall(nextPos)) return false;

        foreach(Transform actor in _actors)
        {
            Vector2 actorGridPos = GetGridPos(actor.transform.position);
            Vector2 playerNextGridPos = GetGridPos(nextPos);

            if(playerNextGridPos != actorGridPos) continue;
            if(actor.TryGetComponent(out Box box))
            {
                if(box.CanBoxMove(movingDir, out _)) activedActors.Add(box); //Maybe use interface method here
                else return false;
            }
        }

        return true;
    }
    #region COROUTINE
    public IEnumerator MovingBoxCoroutine(Vector2 movingDir)
    {
        List<Actor> activedActors = new ();
        List<Coroutine> activedCoroutine = new ();
        if(!CanBoxMove(movingDir,out activedActors)) yield break;

        foreach(var actor in activedActors)
        {
            if(actor.TryGetComponent(out Box box)) //Maybe use interface here
            {
                activedCoroutine.Add(box.StartCoroutine(box.MovingBoxCoroutine(movingDir)));
            }
        }

        yield return StartCoroutine(TranslateCoroutine(movingDir));

        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine));

        yield return null;
    }
    IEnumerator TranslateCoroutine(Vector2 movingDir)
    {
        Vector2 nextPos = Util.GetCertainPosition(transform.position, movingDir);
        Vector2 origPos = transform.position;
        float progress = 0;
        float duration = 0.25f;

        while(progress < duration)
        {
            progress = Mathf.Min(duration, progress + Time.deltaTime);
            transform.position = Vector2.Lerp(origPos, nextPos, progress/duration);
            yield return null;
        }

        Centralize();
        yield return null;
    }
    IEnumerator FallDownAnimation()
    {
        Vector2 floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));

        while(!IsOccupied(floorPos))
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
        }

        yield return null;
    }
    #endregion
}
