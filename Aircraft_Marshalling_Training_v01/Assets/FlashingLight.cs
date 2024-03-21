using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingLight : MonoBehaviour
{
    public float flashDuration = 0.5f; // Duration of each flash
    private Light lightComponent;
    private float timer;

    void Start()
    {
        lightComponent = GetComponent<Light>(); // Get the Light component
    }

    void Update()
    {
        timer += Time.deltaTime; // Increment timer by the time passed since last frame

        if (timer >= flashDuration)
        {
            lightComponent.enabled = !lightComponent.enabled; // Toggle light on/off
            timer = 0; // Reset timer
        }
    }
}