using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Actor
{
    public override bool IsBlocked(Vector2 movingDir){ return !IsOpen; }
    public bool IsOpen = false;
    public void OpenDoor()
    {
        print("OpenDoor!");
        IsOpen = true;
        gameObject.SetActive(false);
    }
    public void CloseDoor()
    {
        print("CloseDoor!");
        IsOpen = false;
        gameObject.SetActive(true);
    }
}
