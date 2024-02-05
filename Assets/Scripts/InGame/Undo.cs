using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo : MonoBehaviour
{
    public class UndoUnit
    {

    }

    public Stack<UndoUnit> UndoStack = new ();

}
