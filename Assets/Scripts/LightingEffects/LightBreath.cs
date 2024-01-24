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
    [SerializeField] float offsetIntensity;

    [Header("Radius")]
    [SerializeField] float meanRadius;
    [SerializeField] float magnitudeRadius;
    [SerializeField] float speedRadius;
    [SerializeField] float offsetRadius;

    Light2D light2D;

    private void FixedUpdate()
    {
        light2D.intensity = meanIntensity + Mathf.Sin(speedIntensity * Time.time - offsetIntensity) * magnitudeIntensity;
        light2D.pointLightOuterRadius = meanRadius + Mathf.Sin(speedRadius * Time.time - offsetRadius) * magnitudeRadius;
    }

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
    }
}
