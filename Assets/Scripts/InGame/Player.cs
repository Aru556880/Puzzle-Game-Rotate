using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;

public class Player : MonoBehaviour
{
    static public Player Instance;
    public Action OnPlayerMovingControlEnd;
    public Action OnPlayerUndoMovement;
    public GameObject CurrentControlActor
    {
        get {return _currentControlActor;}
        set 
        {
            _currentControlActor = value;
            _cinemachine.Follow = value.transform;
        }
    }
    public bool CanPlayerControl;
    
    struct MoveInfo
    {
        GameObject controlActor;
        Vector2Int fromPos;
    }
    [SerializeField] GameObject _currentControlActor;
    [SerializeField] Camera _mainCamera;
    [SerializeField] CinemachineVirtualCamera _cinemachine;
    SpriteRenderer _spriteRenderer;
    void Awake() 
    {
        if(Instance!=null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        _spriteRenderer = GetComponent<SpriteRenderer>();    
    }
    void Start()
    {
        CanPlayerControl = true;
        _cinemachine.Follow = CurrentControlActor.transform;
    }

    private void Update()
    {
        /* TODO: implement this with cinemachine instead: too much shaking to the screen */
        /*Vector3 temp;
        temp = CurrentControlActor.transform.position;
        temp.z = mainCamera.transform.position.z;
        mainCamera.transform.position = temp;*/
    }

    #region PLAYER_RELATED
    public void SwitchMode()
    {
        StartCoroutine(SwitchModeCoroutine());
    }
    #endregion

    #region MOVING_RELATED
    public void Move(Vector2 direction)
    {
        StartCoroutine(PlayerMovingCoroutine(direction));
    }
    #endregion
    

    #region COROUTINE
    IEnumerator SwitchModeCoroutine()
    {
        CanPlayerControl = false;

        if(CurrentControlActor.TryGetComponent(out CharacterFree characterFree))
        {
            characterFree.TryPossess();
        }
        else if(CurrentControlActor.TryGetComponent(out Possessable possessedActor))
        {
            if(possessedActor.IsPossessed(out _)) 
            {
                possessedActor.StopPossessing();
            }
            else Debug.Log("Cannot stop possessing an unpossessed actor!");
        }
        
        yield return null;
        CanPlayerControl = true;
    }
    IEnumerator PlayerMovingCoroutine(Vector2 direction)
    {
        CanPlayerControl = false;

        if(CurrentControlActor.TryGetComponent(out Actor playerActor))
        {
            yield return StartCoroutine(playerActor.ControlActorCoroutine(direction));
        }

        CanPlayerControl = true;
        OnPlayerMovingControlEnd?.Invoke();
    }

    #endregion 
}
