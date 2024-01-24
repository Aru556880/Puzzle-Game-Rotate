using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Lightflickers : MonoBehaviour
{
    [SerializeField] float normalIntensity;
    [SerializeField] float flickerStrength;
    [SerializeField] float flickerFrequency;
    [SerializeField] float flickerLength;

    Light2D light2D;

    private void Start()
    {
        StartCoroutine(Flickerer());
    }

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
    }

    IEnumerator Flickerer()
    {
        while (true)
        {
            if(Random.value < flickerFrequency)
            {
                light2D.intensity -= flickerStrength;
                yield return new WaitForSeconds(flickerLength);
                light2D.intensity = normalIntensity;
            }
            yield return null;
        }
    }
}
