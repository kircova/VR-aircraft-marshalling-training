using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reset : MonoBehaviour
{

    public Vector3 initialPosition;
    public Quaternion initialRotation;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component

    }

    // Update is called once per frame
    public void ResetObject()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (rb != null)
        {
            rb.velocity = Vector3.zero; // Reset the velocity
            rb.angularVelocity = Vector3.zero; // Reset the angular velocity
        }
    }
}
