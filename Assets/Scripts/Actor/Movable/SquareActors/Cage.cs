using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cage : SquareActor
{
    public bool IsLocked;
    public Util.CardinalDirection LockDirection { get {return GetInitCertainDirection(Util.CardinalDirection.Left); }}
    CharacterBody _bodyInCage
    {
        get
        {
            if(!IsLocked) return null;
            foreach(Transform child in transform)
            {
                if(child.TryGetComponent(out CharacterBody charBody))
                {
                    return charBody;
                }
            }

            return null;
        }
    }

    void Start() 
    {
        IsLocked = true;
    }
    public void UnLock()
    {
        if(_bodyInCage!=null)
        {
            Player.Instance.CurrentControlActor = _bodyInCage.gameObject;
            _bodyInCage.transform.SetParent(_actorsTransform);
        }

        IsLocked = false;
    }
    protected override void InteractaWithActors(Vector2 movingDir)
    {
        if(_bodyInCage==null) return;

        base.InteractaWithActors(movingDir);

        Vector2 lockDir = Util.GetVecDirFromCardinalDir(LockDirection);
        List<GameObject> occupyingActors = GetActorsAtPos((Vector2)transform.position + lockDir);
        foreach(GameObject occupyingActor in occupyingActors)
        {
            if(occupyingActor.TryGetComponent(out IInteractableActor interactableActor))
                interactableActor.Interact(this, IInteractableActor.InteractState.Enter, movingDir);
        }
    }
    public override void PerformRotatingAction(Vector2 movingDir) //Keep the character body facing in correct direction
    {
        if(_bodyInCage==null) return;
        _bodyInCage.transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
    }
}
