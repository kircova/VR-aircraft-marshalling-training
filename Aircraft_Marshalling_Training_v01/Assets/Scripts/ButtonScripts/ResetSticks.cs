using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using UnityEngine;

public class ResetSticks : MonoBehaviour
{
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    private Rigidbody rb;
    private NetworkContext context;

    private struct ResetMessage { }
    public bool isOwner;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component

        context = NetworkScene.Register(this);

    }

    public void AttemptReset()
    {
        if (isOwner)
        {
            // If the user is the owner, reset directly.
            ResetObject();
            Debug.Log("Reset done by owner.");

        }
        else
        {
            // If the user is not the owner, send a reset message to the owner.
            context.SendJson(new ResetMessage());
            Debug.Log("Reset send by user.");

        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage m)
    {
        if (isOwner)
        {
            Debug.Log("Reset message received by owner.");

            // Only the owner should listen for reset messages and perform the reset.
            ResetObject();
        }
        else
        {
            Debug.Log("Reset message received by non-owner.");
        }

        
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
