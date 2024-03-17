using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAutomaticResponse : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private float threshold;

    private GameObject LeftStick;
    private GameObject RightStick;
    private bool LeftStickisMoving;
    private bool RightStickisMoving;
    private bool isMoving;
    private bool isBreaking;
    private bool isTurningLeft;
    private bool isTurningRight;

    public int maxHistorySize = 72; // Maximum number of positions to store
    private List<Vector3> leftPositionHistory = new();
    private List<Vector3> rightPositionHistory = new();
    private List<Vector3> leftOrientationHistory = new();
    private List<Vector3> rightOrientationHistory = new();

    // game settings
    [SerializeField] private float motorForce, breakForce, maxSteerAngle;

    // plane wheel colliders
    [SerializeField] private WheelCollider frontWheelCollider;
    [SerializeField] private WheelCollider backLeftWheelCollider, backRightWheelCollider;

    // plane wheels transformations 
    [SerializeField] private Transform frontWheelTransform;
    [SerializeField] private Transform backLeftWheelTransform, backRightWheelTransform;

    public bool isOwner;

    private void Start()
    {
        LeftStick = GameObject.FindGameObjectWithTag("LeftStick");
        RightStick = GameObject.FindGameObjectWithTag("RightStick");

        if (LeftStick == null)
        {
            Debug.Log("Cannot Find GameObject with tag LeftStick.");
        }

        if (RightStick == null)
        {
            Debug.Log("Cannot Find GameObject with tag RightStick.");
        }

        LeftStickisMoving = false;
        RightStickisMoving = false;
        isBreaking = true;
        isMoving = false;
    }

    private void RecordLeftPosition(GameObject stick)
    {
        leftPositionHistory.Add(stick.transform.position);
        leftOrientationHistory.Add(stick.transform.localEulerAngles);
    }

    private void RecordRightPosition(GameObject stick)
    {
        rightPositionHistory.Add(stick.transform.position);
        rightOrientationHistory.Add(stick.transform.eulerAngles);
    }

    private void PlaneMoving()
    {
        threshold = 50;
        // This compares the current orientation with the end orientation (pointing straight down) 
        float left_angle_go = Vector3.Angle(LeftStick.transform.localEulerAngles, new Vector3(90, 0, 45));
        float right_angle_go = Vector3.Angle(RightStick.transform.localEulerAngles, new Vector3(90, 180, 180));

        // Checks if the current position is at the correct height, and is within a threshold of the desired orientation
        if ((LeftStick.transform.position.y < 1.3) && (Mathf.Abs(left_angle_go) < threshold))
        {
            // Now we look through the recent history for the start position of the movement 
            Vector3 orient;
            for (int i = 0; i < maxHistorySize; i++)
            {
                orient = leftOrientationHistory[maxHistorySize - i - 1];
                left_angle_go = Vector3.Angle(orient, new Vector3(270, 0, 0));

                // With an extra condition, that the start position must occur at a point higher than the previous step
                // This is to fix the issue of the plane going immediately after it stops.
                if ((leftPositionHistory[maxHistorySize - i - 1].y > 1.7)&&(Mathf.Abs(left_angle_go) < threshold))
                {
                    LeftStickisMoving = true;
                    break;
                }
            }
        }

        // We only check the RightStick position if the LeftStick is in the correct position
        if ((LeftStickisMoving) && (RightStick.transform.position.y < 1.3) && (Mathf.Abs(right_angle_go) < threshold))
        {
            
            Vector3 orient;
            for (int i = 1; i < maxHistorySize; i++)
            {
                orient = rightOrientationHistory[i];
                right_angle_go = Vector3.Angle(orient, new Vector3(270, 0, 0));
                // Same conditions as for the left stick 
                if ((rightPositionHistory[i].y > 1.7) && (Mathf.Abs(right_angle_go) < threshold))
                {
                    RightStickisMoving = true;
                    isMoving = true;
                    isBreaking = false;
                    break;
                }
            }
            // Here, we say that if only the left stick is doing the motion, then both need to be set to false. 
            if (!RightStickisMoving)
            {
                LeftStickisMoving = false;
                isBreaking = true;
                isMoving = false;
            }
        }

    }

    private void PlaneStopping()
    {
        threshold = 40;
        // This compares the current orientation with the end orientation (sticks crossed above head) 
        float left_angle_stop = Vector3.Angle(LeftStick.transform.localEulerAngles, new Vector3(300, 90, 180));
        float right_angle_stop = Vector3.Angle(RightStick.transform.localEulerAngles, new Vector3(300, 180, 180));

        // Check for stop if the plane is moving and the end of the stop movement occurs 
        if ((LeftStick.transform.position.y > 1.7) && (Mathf.Abs(left_angle_stop) < threshold))
        {
            // Now check for the starting position of the forward motion (arms out to the sides) 
            Vector3 orient;
            for (int i = 0; i < maxHistorySize; i++)
            {
                orient = leftOrientationHistory[maxHistorySize - i - 1];
                // TODO CHECK THIS ANGLE
                left_angle_stop = Vector3.Angle(orient, new Vector3(0, 250, 0));
                if ((leftPositionHistory[i].y < 1.5) && (Mathf.Abs(left_angle_stop) < threshold))
                {
                    LeftStickisMoving = false;
                    break;
                }
            }
        }

        // Only check the RightStick if the LeftStick is in the correct position 
        if ((!LeftStickisMoving) && (RightStick.transform.position.y > 1.7) && (Mathf.Abs(right_angle_stop) < threshold))
        {
            // Now check for the starting position of the forward motion
            Vector3 orient;
            for (int i = 0; i < maxHistorySize; i++)
            {
                orient = rightOrientationHistory[i];
                right_angle_stop = Vector3.Angle(orient, new Vector3(0, 70, 0));

                if ((rightPositionHistory[i].y < 1.5) && (Mathf.Abs(right_angle_stop) < threshold))
                {
                    RightStickisMoving = false;
                    break;
                }
            }
            // Here, we say that if only the left stick is doing the stop motion, then the plane does not stop. 
            if (RightStickisMoving)
            {
                LeftStickisMoving = true;
                isMoving = true;
                isBreaking = false;
            }
        }

        if ((!LeftStickisMoving) && (!RightStickisMoving))
        {
            isBreaking = true;
            isMoving = false;
        }
    }

    private void PlaneTurningLeft()
    {
        float threshold1 = 60;
        float threshold2 = 50;

        float right_angle_left = Vector3.Angle(RightStick.transform.localEulerAngles, new Vector3(0, 70, 0));
        float left_angle_left = Vector3.Angle(LeftStick.transform.localEulerAngles, new Vector3(300, 90, 180));

        if ((Mathf.Abs(right_angle_left) < threshold1) && (Mathf.Abs(left_angle_left) < threshold2))
        {
            if (LeftStick.transform.localPosition.y - RightStick.transform.localPosition.y > 0.1)
            {
                isTurningLeft = true; 
            }
        }
    }

    private void PlaneTurningRight()
    {
        float threshold1 = 60;
        float threshold2 = 50;

        float left_angle_right = Vector3.Angle(LeftStick.transform.localEulerAngles, new Vector3(0, 250, 0));
        float right_angle_right = Vector3.Angle(RightStick.transform.localEulerAngles, new Vector3(300, 180, 180));

        if ((Mathf.Abs(right_angle_right) < threshold2) && (Mathf.Abs(left_angle_right) < threshold1))
        {
            if (RightStick.transform.localPosition.y - LeftStick.transform.localPosition.y > 0.1)
            {
                isTurningRight = true;
            }
        }
    }

    private void PlaneStopTurningLeft()
    {
        if ((LeftStick.transform.localPosition.y - RightStick.transform.localPosition.y <= -0.1) || (LeftStick.transform.localPosition.y < 1.4))
        {
            isTurningLeft = false;
        }
    }

    private void PlaneStopTurningRight()
    {
        if ((RightStick.transform.localPosition.y - LeftStick.transform.localPosition.y <= -0.1) || (RightStick.transform.localPosition.y < 1.4))
        {
            isTurningRight = false;
        }
    }

    private void FixedUpdate()
    {

        // Add current position to history
        RecordLeftPosition(LeftStick);
        RecordRightPosition(RightStick);

        // Optional: Trim history to maxHistorySize
        if (leftPositionHistory.Count > maxHistorySize)
        {
            leftPositionHistory.RemoveAt(0);
            leftOrientationHistory.RemoveAt(0);
            rightPositionHistory.RemoveAt(0);
            rightOrientationHistory.RemoveAt(0);
        }

        // Only want to do one thing per time step!
        if (!isMoving)
        {
            PlaneMoving();
        }
        else if (isMoving && (!isTurningLeft) && (!isTurningRight))
        {
            PlaneStopping();
            if (!isBreaking)
            {
                PlaneTurningRight();
                if (!isTurningRight)
                {
                    PlaneTurningLeft();
                }
            }
        }
        else if (isMoving && isTurningRight)
        {
            PlaneStopTurningRight();
        }
        else if (isMoving && isTurningLeft)
        {
            PlaneStopTurningLeft();
        }

        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();

        //if (isMoving && isBreaking)
        //{
        //    Debug.Log("isMoving AND isBreaking !!!!");
        //}
        //if ((!isMoving) && (!isBreaking))
        //{
        //    Debug.Log(" NEITHER isMoving OR isBreaking !!!!");
        //}

        //if ((isTurningLeft) && (isTurningRight))
        //{
        //    Debug.Log(" BOTH isTurningLeft AND isTurningRight !!!!");
        //}

        //if (isMoving)
        //{
        //    Debug.Log("isMoving");
        //}
        //if (isTurningLeft)
        //{
        //    Debug.Log("Left Turn");
        //}
        //if (isTurningRight)
        //{
        //    Debug.Log("Right Turn");
        //}
        //if (isBreaking)
        //{
        //    Debug.Log("Break!");
        //}
    }

    private void GetInput()
    {
        
        // input from steering

        if (isTurningLeft)
        {
            horizontalInput = -1;
        } else if (isTurningRight)
        {
            horizontalInput = 1;
        } else
        {
            horizontalInput = 0;
        }


        if (isMoving)
        {
            verticalInput = -1;
        }
        else
        {
            verticalInput = 0;
        }
        
    }

    private void HandleMotor()
    {
        backLeftWheelCollider.motorTorque = verticalInput * motorForce;
        backRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentbreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontWheelCollider.brakeTorque = currentbreakForce;
        backLeftWheelCollider.brakeTorque = currentbreakForce;
        backRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateOneWheel(frontWheelCollider, frontWheelTransform);
        UpdateOneWheel(backRightWheelCollider, backRightWheelTransform);
        UpdateOneWheel(backLeftWheelCollider, backLeftWheelTransform);
    }

    private void UpdateOneWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}
