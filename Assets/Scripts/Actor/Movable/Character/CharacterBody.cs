using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBody : MovableActor
{
    public bool IsEmpty = false;
    [SerializeField] Sprite PlayerFullBodySprite;
    [SerializeField] Sprite PlayerEmptyBodySprite;
    SpriteRenderer _spriteRenderer;
    void Awake() 
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();    
    }
    public override void BePossessed(CharacterFree possessingChar)
    {
        if(!IsPossessed(out _)) 
        {
            base.BePossessed(possessingChar);
            IsEmpty = false;
        }
    }
    public override void StopPossessing()
    {   
        if(IsPossessed(out CharacterFree possessingChar))
        {
            base.StopPossessing();
            IsEmpty = true;
        }
    }
}
