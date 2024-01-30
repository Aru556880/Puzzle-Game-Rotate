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

    #region OVERRIDE_POSSESS_RELATED
    //Possess the cage is actually possess the characterBody inside it, need to override something
    public override bool IsPossessed(out CharacterFree possessingChar) //It is possessed if the body inside it is possessed
    {
        if(_bodyInCage!=null && _bodyInCage.IsPossessed(out CharacterFree charFree))
        {
            possessingChar = charFree;
            return true;
        }

        possessingChar = null;
        return false;
    }
    public override bool CanBePossessed //Cage can be possessed if it is locked and contains character body which is empty
    {
        get
        {
            if(_bodyInCage!=null && _bodyInCage.CanBePossessed) return true;

            return false;
        }
    }
    public override void BePossessed(CharacterFree possessingChar)
    {
        if(_bodyInCage!=null && !IsPossessed(out _))
        {
            _bodyInCage.BePossessed(possessingChar);
            Player.Instance.CurrentControlActor = gameObject;
        }
    }
    public override void StopPossessing()
    {   
        if(IsPossessed(out CharacterFree possessingChar))
        {
            _bodyInCage.StopPossessing();
            possessingChar.transform.position = transform.position; 
            Player.Instance.CurrentControlActor = possessingChar.gameObject;
        }
    }
    #endregion
    public override IEnumerator MovedByPlayerCoroutine(Vector2 direction)
    {
        if(_bodyInCage==null) yield break;
        yield return StartCoroutine(base.MovedByPlayerCoroutine(direction));
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
