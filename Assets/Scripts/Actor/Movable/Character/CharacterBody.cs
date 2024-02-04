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
            _spriteRenderer.sprite = PlayerFullBodySprite;
            IsEmpty = false;
        }
    }
    public override void StopPossessing()
    {   
        if(IsPossessed(out CharacterFree possessingChar))
        {
            base.StopPossessing();
            _spriteRenderer.sprite = PlayerEmptyBodySprite;
            IsEmpty = true;
        }
    }

    #region IMPLEMENT_ABSTRACT_METHODS
    public override IEnumerator MovedByPlayerCoroutine(Vector2 direction)
    {
        //When player control this block and input WASD, this coroutine will be called
        if(Mathf.Abs(direction.x) < 0.5f)
        {
            direction.x = 0;
        }
        else 
        {
            direction.y = 0;
        }

        direction.Normalize();
        Vector2 nextPos = Util.GetCertainPosition(transform.position, direction);
        List<Coroutine> activedCoroutine = new ();

        if(!CanMoveWhenControlled(direction, Vector2.zero))
        {
            yield break;
        }
        
        activedCoroutine = Util.MergeList(activedCoroutine, PushActorsCoroutines(nextPos, direction));

        yield return StartCoroutine(TranslatingAnimation(direction));

        InteractaWithActors(direction);

        activedCoroutine = Util.MergeList(activedCoroutine, FallingActorsCoroutines());

        yield return StartCoroutine(Util.WaitForCoroutines(activedCoroutine)); //Wait for other coroutines finished

    }
    #endregion
}
