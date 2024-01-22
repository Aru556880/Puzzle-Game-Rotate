using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    // This class defines a parallax layer and its properties. Actual rendering is done by ParallaxController.

    // distance defines how much it should be slowed reletive to the player.
    // 0 is same speed (not moving in the scene), 1 is still image (move with camera).
    // It is possible to make ths value over 1 or under 0 for specific effects.
    public float distance;

    public Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.position;
    }
}
