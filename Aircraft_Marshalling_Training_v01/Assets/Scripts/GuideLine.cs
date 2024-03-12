using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideLineLight : MonoBehaviour
{
    public GameObject Plane;
    private bool isOnLine;

    public Material lineMaterial;


    // Start is called before the first frame update
    void Start()
    {
        isOnLine = false;

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ClientPlane"))
        {
            Debug.Log("The Plane is on the line " + other.name);
            isOnLine = true;
            lineMaterial.SetColor("_Color", Color.green);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ClientPlane"))
        {
            Debug.Log("The Plane is off the line " + other.name);
            isOnLine = false;
            lineMaterial.SetColor("_Color", Color.red);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        lineMaterial.SetColor("_Color", Color.green);
    }


}
