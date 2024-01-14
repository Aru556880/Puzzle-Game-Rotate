using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MovableActor
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public bool CanBoxMove(Vector2 movingDir)
    {
        Vector2 currentPos = transform.position;
        Vector2 nextPos = Util.GetCertainPosition(currentPos, movingDir);

        if(movingDir.x == 0 && movingDir.y == 0) return false;
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
    #region COROUTINE
    public IEnumerator MovingBoxCoroutine(Vector2 movingDir)
    {
        List<Coroutine> activedCoroutine = new ();
        Vector2 nextPos = Util.GetCertainPosition(transform.position, movingDir);

        if(!CanBoxMove(movingDir)) yield break;

        activedCoroutine = Util.MergeList(activedCoroutine, PushActors(nextPos, movingDir));

        yield return StartCoroutine(TranslatingBoxCoroutine(movingDir));

        TriggetInteractableActors();

        activedCoroutine = Util.MergeList(activedCoroutine, FallDownActors());

        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine));

        yield return null;
    }
    IEnumerator TranslatingBoxCoroutine(Vector2 movingDir)
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
    #endregion
}
