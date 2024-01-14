using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableActor : Actor
{
    protected bool _isFalling = false;
    public IEnumerator FallDownAnimation()
    {
        if(_isFalling) yield break;

        _isFalling = true;
        Vector2 floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));

        while(WillFallDown())
        {
            Vector2 origPos = transform.position;
            float progress = 0;
            float duration = 0.1f;
            while(progress < duration)
            {
                progress = Mathf.Min(duration, progress + Time.deltaTime);
                transform.position = Vector2.Lerp(origPos, floorPos, progress/duration);
                yield return null;
            }

            Centralize();
            floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));
        }

        List<GameObject> fallingActors = new ();
        List<Coroutine> activedCoroutine = new ();
        fallingActors = GetFallingActor();
        foreach(var actor in fallingActors)
        {
            if(actor.TryGetComponent(out Box box)) //Maybe use interface here
            {
                Coroutine coroutine = box.StartCoroutine(box.FallDownAnimation());
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
            else if(actor.TryGetComponent(out Player player))
            {
                Coroutine coroutine = player.StartCoroutine(player.FallDownAnimation());
                coroutine = player.StartCoroutine(player.FallDownAnimation());
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        TriggetInteractableActors();
        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine));
        _isFalling = false;
    }
    public virtual bool WillFallDown()
    {
        Vector2 floorPos = Util.GetCertainPosition(transform.position, new Vector2(0,-1));
        return !IsOccupied(floorPos);
    }
    protected List<GameObject> GetFallingActor()
    {
        List<GameObject> actorList = new ();
        foreach(Transform child in _actors)
        {
            if(child.TryGetComponent(out MovableActor actor) && actor.WillFallDown())
            {
                if(!actorList.Contains(child.gameObject)) actorList.Add(child.gameObject);
            }
        }

        return actorList;
    }
    protected void TriggetInteractableActors()
    {
        List<GameObject> occupyingActors = GetActorsAtPos(transform.position);
        foreach(GameObject occupyingActor in occupyingActors)
        {
            if(occupyingActor.TryGetComponent(out IInteractableActor interactableActor))
                interactableActor.Interact(this);
        }
    }
    #region METHOD_INMOVING
    protected List<Coroutine> PushActors(Vector2 pushedPos, Vector2 movingDir) 
    {
        //return coroutine of all moving pushed boxes
        
        List<GameObject> pushedActors = new ();
        List<Coroutine> activedCoroutine = new ();

        pushedActors = GetActorsAtPos(pushedPos);
        foreach(var actor in pushedActors)
        {
            if(actor.TryGetComponent(out Box box)) //Maybe use interface here
            {
                Coroutine coroutine = box.StartCoroutine(box.MovingBoxCoroutine(movingDir));
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        return activedCoroutine;
    }
    protected List<Coroutine> FallDownActors()
    {
        List<GameObject> fallingActors = GetFallingActor();
        List<Coroutine> activedCoroutine = new ();

        foreach(var actor in fallingActors)
        {
            if(actor.TryGetComponent(out Box box)) //Maybe use interface here
            {
                Coroutine coroutine = box.StartCoroutine(box.FallDownAnimation());
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
            else if(actor.TryGetComponent(out Player player))
            {
                Coroutine coroutine = player.StartCoroutine(player.FallDownAnimation());
                if(!activedCoroutine.Contains(coroutine)) activedCoroutine.Add(coroutine);
            }
        }

        return activedCoroutine;
    }
    #endregion 
}
