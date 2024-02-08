using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightObj : MovableActor
{
    public Util.CardinalDirection LightDirection { get {return GetInitCertainDirection(Util.CardinalDirection.Left); }}
    public bool IsLightBlocked()
    {
        Vector2 lightVecDir = Util.GetVecDirFromCardinalDir(LightDirection);
        Vector2 targetPos = Util.GetCertainPosition(transform.position, lightVecDir);

        return IsOccupiedAt(targetPos);
    }
}
