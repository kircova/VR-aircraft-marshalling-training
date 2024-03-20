using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Spawning;
using UnityEngine;

public class PlaneNetworking : MonoBehaviour//, INetworkSpawnable
{
    NetworkContext context;
    Transform parent;
    //public NetworkId NetworkId { get; set; }

    public bool isOwner;

    void Start()
    {
        parent = transform.parent;
        context = NetworkScene.Register(this);
        // only client is in charge of movement
    }

    private struct Message
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    void Update()
    {
        if(isOwner)
        {
            Message m = new Message();
            m.position = this.transform.localPosition;
            m.rotation = this.transform.localEulerAngles;
            context.SendJson(m);
        }
        
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage m)
    {
        var message = m.FromJson<Message>();
        transform.localPosition = message.position;
        transform.localEulerAngles = message.rotation;
        //Debug.Log(gameObject.name + " Updated");
    }
}
