using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cage : MovableActor
{
    public bool IsLocked;
    public Util.CardinalDirection LockDirection { get {return GetInitCertainDirection(Util.CardinalDirection.Left); }}
    [SerializeField] Character character;
    void Start() 
    {
        IsLocked = true;
    }
    protected override void TriggetInteractableActors()
    {
        base.TriggetInteractableActors();

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
