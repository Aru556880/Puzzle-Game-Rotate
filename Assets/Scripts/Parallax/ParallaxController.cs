using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] List<ParallaxLayer> layers;

    Vector3 originalPosition;
    Vector3 offset;

    private void Start()
    {
        originalPosition = _camera.transform.position;
    }

    private void FixedUpdate()
    {
        // get camera offset
        offset = _camera.transform.position - originalPosition;
        offset.z = 0;

        // move each layer reletive to offset
        foreach(ParallaxLayer layer in layers)
        {
            layer.transform.position = layer.originalPosition + offset * layer.distance;
        }
    }
}
