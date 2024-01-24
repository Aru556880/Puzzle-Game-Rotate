using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightBreath : MonoBehaviour
{
    [Header("Intensity")]
    [SerializeField] float meanIntensity;
    [SerializeField] float magnitudeIntensity;
    [SerializeField] float speedIntensity;

    [Header("Radius")]
    [SerializeField] float meanRadius;
    [SerializeField] float magnitudeRadius;
    [SerializeField] float speedRadius;

    private void FixedUpdate()
    {
        GetComponent<Light2D>().intensity = meanIntensity + Mathf.Sin(speedIntensity * Time.time) * magnitudeIntensity;
        GetComponent<Light2D>().pointLightOuterRadius = meanRadius + Mathf.Sin(speedRadius * Time.time) * magnitudeRadius;
    }
}
