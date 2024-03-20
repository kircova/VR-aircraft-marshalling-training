using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking;

    // game settings
    [SerializeField] private float motorForce, breakForce, maxSteerAngle, maxSpeed;

    // plane wheel colliders
    [SerializeField] private WheelCollider frontWheelCollider;
    [SerializeField] private WheelCollider backLeftWheelCollider, backRightWheelCollider;

    // plane wheels transformations 
    [SerializeField] private Transform frontWheelTransform;
    [SerializeField] private Transform backLeftWheelTransform, backRightWheelTransform;

    public bool isOwner;

    private void Start()
    {
        maxSpeed = 2; 
    }

    private void SpeedLimit()
    {
        GameObject Plane = GameObject.FindGameObjectWithTag("ClientPlane");
        Rigidbody rb = Plane.GetComponent<Rigidbody>();

        float speed = rb.velocity.magnitude;

        if (speed > maxSpeed)
        {
            Vector3 clampedV = rb.velocity.normalized * maxSpeed;
            rb.velocity = clampedV;
        }
    }

    private void FixedUpdate() {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        SpeedLimit();
    }

    private void GetInput() {
        // input from steering
        horizontalInput = Input.GetAxis("Horizontal");

        // input from acceletation
        verticalInput = Input.GetAxis("Vertical");

        // input from breaking
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor() {
        backLeftWheelCollider.motorTorque = verticalInput * motorForce;
        backRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentbreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking() {
        frontWheelCollider.brakeTorque = currentbreakForce;
        backLeftWheelCollider.brakeTorque = currentbreakForce;
        backRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering() {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels() {
        UpdateOneWheel(frontWheelCollider, frontWheelTransform);
        UpdateOneWheel(backRightWheelCollider, backRightWheelTransform);
        UpdateOneWheel(backLeftWheelCollider, backLeftWheelTransform);
    }

    private void UpdateOneWheel(WheelCollider wheelCollider, Transform wheelTransform) {
        Vector3 pos;
        Quaternion rot; 
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}
