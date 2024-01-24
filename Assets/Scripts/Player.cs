using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;

public class Player : MonoBehaviour
{
    static public Player Instance;
    public Vector2Int HeadDirection;
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
    
    [SerializeField] GameObject _currentControlActor;
    SpriteRenderer _spriteRenderer;
    [SerializeField] Camera _mainCamera;
    [SerializeField] CinemachineVirtualCamera _cinemachine;
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
        HeadDirection = new Vector2Int(1,0);
        CanPlayerControl = true;
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
        else if(CurrentControlActor.TryGetComponent(out MovableActor movableActor))
        {
            if(movableActor.IsPossessed(out _)) 
            {
                movableActor.StopPossessing();
            }
            else Debug.Log("Cannot stop possessing an unpossessed actor!");
        }
        
        yield return null;
        CanPlayerControl = true;
    }
    IEnumerator PlayerMovingCoroutine(Vector2 direction)
    {
        CanPlayerControl = false;

        if(CurrentControlActor.TryGetComponent(out MovableActor movableActor))
        {
            yield return StartCoroutine(movableActor.MovedByPlayerCoroutine(direction));
        }
        else if(CurrentControlActor.TryGetComponent(out CharacterFree characterFree))
        {
            print("TODO: Free Character Moving");
        }

        CanPlayerControl = true;
    }

    #endregion 
}
