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
            if(!IsLocked) return false;

            if(_bodyInCage!=null && _bodyInCage.CanBePossessed) return true;

            return false;
        }
    }
    public override void BePossessed(CharacterFree possessingChar)
    {
        if(!IsPossessed(out _))
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
    protected override void TriggerInteractableActors()
    {
        base.TriggerInteractableActors();

        Vector2 lockDir = Util.GetVecDirFromCardinalDir(LockDirection);
        List<GameObject> occupyingActors = GetActorsAtPos((Vector2)transform.position + lockDir);
        foreach(GameObject occupyingActor in occupyingActors)
        {
            if(occupyingActor.TryGetComponent(out IInteractableActor interactableActor))
                interactableActor.Interact(this, lockDir);
        }
    }
    public override void PerformRotatingAction(Vector2 movingDir) //Keep the character body facing in correct direction
    {
        _bodyInCage.transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
    }
}
