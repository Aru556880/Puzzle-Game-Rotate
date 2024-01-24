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
    #region OVERRIDE
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
            possessingChar.transform.SetParent(_bodyInCage.transform);
            possessingChar.gameObject.SetActive(false);
            _bodyInCage.BePossessed(possessingChar);
            Player.Instance.CurrentControlActor = gameObject;
        }
    }
    public override void StopPossessing()
    {   
        if(IsPossessed(out CharacterFree possessingChar))
        {
            possessingChar.gameObject.SetActive(true);
            possessingChar.transform.SetParent(GameManager.Instance.levelBuilder.AllActors.transform);
            possessingChar.transform.position = transform.position; 
            _bodyInCage.StopPossessing();
            Player.Instance.CurrentControlActor = possessingChar.gameObject;
        }
    }
    #endregion
    void Start() 
    {
        IsLocked = true;
    }
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
    public void UnLock()
    {
        IsLocked = false;
    }
}
