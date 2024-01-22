using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    static public Player Instance;
    public Vector2Int HeadDirection;
    public MovableActor CurrentControlActor;
    public bool CanPlayerControl;
    
    SpriteRenderer _spriteRenderer;
    [SerializeField] Camera mainCamera;
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
        Vector3 temp;
        temp = CurrentControlActor.transform.position;
        temp.z = mainCamera.transform.position.z;
        mainCamera.transform.position = temp;
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

        yield return null;
        
        CanPlayerControl = true;
    }
    IEnumerator PlayerMovingCoroutine(Vector2 direction)
    {
        CanPlayerControl = false;

        yield return StartCoroutine(CurrentControlActor.MovedByPlayerCoroutine(direction));

        CanPlayerControl = true;
    }

    #endregion 
}
