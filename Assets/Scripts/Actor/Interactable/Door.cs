using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Actor
{
    public override bool IsBlocked(Vector2 movingDir){ return true; }
    public void OpenDoor()
    {
        print("OpenDoor!");
        gameObject.SetActive(false);
    }
    public void CloseDoor()
    {
        print("CloseDoor!");
        gameObject.SetActive(true);
    }
}
