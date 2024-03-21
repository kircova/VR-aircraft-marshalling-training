using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TabletNetworking : MonoBehaviour
{

    NetworkContext context;
    Transform parent;

    IXRInteractable interactable;
    public bool isInteractedWith;

    public int token;

    // Does this instance of the Component control the transforms for everyone?
    public bool isOwner;

    public bool isGrabbed;
    
    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent;
        interactable = GetComponent<IXRInteractable>();
        var baseInteractable = interactable as XRBaseInteractable;

        if (baseInteractable != null)
        {
            baseInteractable.firstSelectEntered.AddListener(OnInteractedStart);
            baseInteractable.lastSelectExited.AddListener(OnInteractedEnd);
        }

        context = NetworkScene.Register(this);
        token = Random.Range(1, 10000);
        isOwner = true; // Start by both exchanging the random tokens to see who wins...
        isGrabbed = false;
    }
    
    void OnInteractedStart(SelectEnterEventArgs ev)
    {
        Debug.Log("Picked up");
        TakeOwnership();
    }

    void OnInteractedEnd(SelectExitEventArgs ev)
    {
        Debug.Log("Dropped");
        isInteractedWith = false;
        transform.parent = parent;
        GetComponent<Rigidbody>().isKinematic = false;
    }


    private struct Message
    {
        public Vector3 position;
        public Vector3 rotation;
        public int token;
    }


    void TakeOwnership()
    {
        token++;
        isOwner = true;
        isInteractedWith = true;
    }

    void Update()
    {
        if(isOwner)
        {
            Message m = new Message();
            m.position = this.transform.localPosition;
            m.rotation = this.transform.localEulerAngles;
            m.token = token;
            context.SendJson(m);
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage m)
    {
        var message = m.FromJson<Message>();
        transform.localPosition = message.position;
        transform.localEulerAngles = message.rotation;
        if(message.token > token)
        {
            isOwner = false;
            isInteractedWith = false;
            token = message.token;
            GetComponent<Rigidbody>().isKinematic = true;
        }
        Debug.Log(gameObject.name + " Updated");
    }
}
