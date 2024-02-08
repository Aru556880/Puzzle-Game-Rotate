using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static Transform ActorsTransform { get { return Instance.levelBuilder.AllActors.transform; } }
    public LevelBuilder levelBuilder;
    private bool isMovementKeysPressed;
    private Player _player;
    void Awake() 
    {
        if(Instance==null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }
    void Start()
    {
        _player = FindObjectOfType<Player>();
        levelBuilder = FindObjectOfType<LevelBuilder>();
        isMovementKeysPressed = false;
    }
    #region INPUT_EVENT
    public void TryUndo(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            _player.OnPlayerUndoMovement?.Invoke();
        }
    }
    public void TryPlayerMove(InputAction.CallbackContext context)
    {
        StopCoroutine("KeepPressingKey");
        
        if(context.performed) 
        {
            isMovementKeysPressed = true;
            StartCoroutine(KeepPressingKey(context));
        }

        if(context.canceled)
        {
            isMovementKeysPressed = false;
        }
            
    }
    public void TryPlayerSwitchMode(InputAction.CallbackContext context)
    {
        if(context.performed && !isMovementKeysPressed && _player.CanPlayerControl)
        {
            _player.SwitchMode();
        }
    }
    #endregion

    #region COROUTINE
    IEnumerator KeepPressingKey(InputAction.CallbackContext context)
    {
        Vector2 movementInput;
        while(isMovementKeysPressed)
        {
            movementInput = context.ReadValue<Vector2>();
            movementInput.Normalize();

            if(_player.CanPlayerControl) _player.Move(movementInput);
            yield return null;
        }
    }
    #endregion
}
