using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using UnityEngine;

public class PlaneNetworking : MonoBehaviour
{
    NetworkContext context;
    Transform parent;

    public bool isOwner;

    void Start()
    {
        parent = transform.parent;
        context = NetworkScene.Register(this);
        // only client is in charge of movement
        if(gameObject.tag == "ClientPlane") {
            isOwner = true;
        } else {
            isOwner = false;
        }
    }

    private struct Message
    {
        public Vector3 position;
    }

    void Update()
    {
        if(isOwner)
        {
            Message m = new Message();
            m.position = this.transform.localPosition;
            context.SendJson(m);
        }
        
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage m)
    {
        var message = m.FromJson<Message>();
        transform.localPosition = message.position;
        Debug.Log(gameObject.name + " Updated");
    }
}
