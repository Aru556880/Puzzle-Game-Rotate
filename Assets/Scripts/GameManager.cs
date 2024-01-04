using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public LevelBuilder levelBuilder;
    private bool _readyForInput;
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
    }

    void Update()
    {
        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        movementInput.Normalize();

        if(_player)
        {
            if(IsKeysReleased()) _readyForInput = true;

            if(_readyForInput && IsKeyPressed())
            {
                _readyForInput = false;
                _player.Move(movementInput);
            }
        }
    }
    bool IsKeyPressed()
    {
        return !IsKeysReleased();
    }
    bool IsKeysReleased()
    {
        return !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D);
    }
}
