using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : Actor, IInteractableActor
{
    public bool CanInteract => throw new System.NotImplementedException();

    public void Interact(Actor actor, Vector2 triggerDir, Vector2 movingDir)
    {
        if(triggerDir == Vector2.zero && actor.TryGetComponent(out CharacterBody characterBody))
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
        yield return new WaitForSeconds(1f);

        LevelManager.Instance.EnterLevelTest();
    }

}
