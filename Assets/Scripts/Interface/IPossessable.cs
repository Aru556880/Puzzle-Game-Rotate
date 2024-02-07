using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPossessable
{
    public bool IsPossessed { get;  }
    public bool CanPossessable { get; }
    public void BePossessed();
    public void StopPossessing();
}
