using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : Actor, IInteractableActor
{
    public bool CanInteract => throw new System.NotImplementedException();

    public void Interact(Actor actor, IInteractableActor.InteractState state, Vector2 movingDir)
    {
        if(state == IInteractableActor.InteractState.Enter && actor.TryGetComponent(out CharacterBody characterBody))
        {
            //Character wall out the screen and Go to Next Level
            Player.Instance.OnPlayMovingControlEnd += ()=> 
            {
                Player.Instance.CanPlayerControl = false;
                StartCoroutine(EndAnimCoroutine(characterBody, movingDir));
            };
        }
    }
    IEnumerator EndAnimCoroutine(CharacterBody characterBody, Vector2 direction)
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(characterBody.MovedByPlayerCoroutine(direction));
        LevelManager.Instance.EnterLevelTest();
    }

}
