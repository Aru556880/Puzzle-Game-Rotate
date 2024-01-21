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

}
