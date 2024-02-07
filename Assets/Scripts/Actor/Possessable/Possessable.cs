using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attach this class to the actor so that player can possess it
public class Possessable : MonoBehaviour
{
    protected Transform _actorsTransform { get { return GameManager.Instance.levelBuilder.AllActors.transform; } }
    public virtual bool IsPossessed(out CharacterFree possessingChar) 
    {
        foreach(Transform child in transform)
        {
            if(child.TryGetComponent(out CharacterFree charFree))
            {
                possessingChar = charFree;
                return true;
            }
        }
        
        possessingChar = null;
        return false;
    }
    public virtual bool CanBePossessed{ get{ return !IsPossessed(out _); }}
    public virtual void BePossessed(CharacterFree possessingChar) //Now we control this movable actor
    {
        if(!IsPossessed(out _)) 
        {
            possessingChar.transform.SetParent(transform);
            possessingChar.gameObject.SetActive(false);
            Player.Instance.CurrentControlActor = gameObject;
        }
    }
    public virtual void StopPossessing() //Leave the possessed movable actor
    {   
        if(IsPossessed(out CharacterFree possessingChar))
        {
            possessingChar.gameObject.SetActive(true);
            possessingChar.transform.SetParent(_actorsTransform);
            possessingChar.transform.position = transform.position;  //The free character is spawned at this block
            Player.Instance.CurrentControlActor = possessingChar.gameObject;
        }
    }
}
