using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;


public class PlaneAutomaticResponse : MonoBehaviour
{

    private NetworkContext context;
    public bool isRunning; // This determines whether we have started running the simulation 

    public bool sticksInHands; // This determines whether the Marshal is holding sticks

    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private float threshold;

    private GameObject Marshal1;
    private GameObject Marshal2;
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
        context = NetworkScene.Register(this);

        LeftStick = GameObject.FindGameObjectWithTag("LeftStick");
        RightStick = GameObject.FindGameObjectWithTag("RightStick");
        Marshal1 = GameObject.FindGameObjectWithTag("Marshal1");
        Marshal2 = GameObject.FindGameObjectWithTag("Marshal2");

        

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
        isBreaking = true; // this might be unnecessary, we can probably remove it and use isMoving for both 
        isMoving = false;
        isRunning = true; // TODO: this needs to be updated and linked with the button!
        sticksInHands = false;
        
         // TODO: this needs to be updated - from the controller information I believe (have they grabbed and not dropped etc) 
    }

    private void RecordLeftPosition(GameObject stick)
    {
        // Keep track of recent LeftStick position and orientation
        // so that we can search for start positions of movements 
        leftPositionHistory.Add(stick.transform.position);
        leftOrientationHistory.Add(stick.transform.localEulerAngles);
    }

    private void RecordRightPosition(GameObject stick)
    {
        // Keep track of recent RightStick position and orientation
        // so that we can search for start positions of movements 
        rightPositionHistory.Add(stick.transform.position);
        rightOrientationHistory.Add(stick.transform.eulerAngles);
    }

    private void PlaneMoving()
    {
        // This function checks if the sticks (both sticks) are currently in the end position of the GO movement.
        // It then searches through recent history, to see if both have been in the start position of the GO movement.
        // If they have, then it turns on the plane forward motion.

        //threshold = 50;
        threshold = 60;
        // This compares the current orientation with the end orientation (sticks points down) 
        float left_angle_go = Vector3.Angle(LeftStick.transform.localEulerAngles, new Vector3(90, 0, 45));
        float right_angle_go = Vector3.Angle(RightStick.transform.localEulerAngles, new Vector3(90, 180, 180));

        // Left Stick: Check if the current position is at the correct height, and is within a threshold of the desired orientation
        if ((LeftStick.transform.position.y < 1.3) && (Mathf.Abs(left_angle_go) < threshold))
        {
            // Now search for the start GO position 
            Vector3 orient;
            for (int i = 0; i < maxHistorySize; i++)
            {
                orient = leftOrientationHistory[maxHistorySize - i - 1];
                left_angle_go = Vector3.Angle(orient, new Vector3(270, 0, 0));
                // height used to be 1.7 changed to 1.6 for metaquest 2
                if ((leftPositionHistory[maxHistorySize - i - 1].y > 1.6)&&(Mathf.Abs(left_angle_go) < threshold))
                {
                    LeftStickisMoving = true;
                    break;
                }
            }
        }

        // Right Stick: We only check the RightStick position if the LeftStick is in the correct position
        // Now check for the end position of the GO movement 
        if ((LeftStickisMoving) && (RightStick.transform.position.y < 1.3) && (Mathf.Abs(right_angle_go) < threshold))
        {
            // Check for the start position of the GO movement 
            Vector3 orient;
            for (int i = 1; i < maxHistorySize; i++)
            {
                orient = rightOrientationHistory[i];
                right_angle_go = Vector3.Angle(orient, new Vector3(270, 0, 0));
                if ((rightPositionHistory[i].y > 1.7) && (Mathf.Abs(right_angle_go) < threshold))
                {
                    RightStickisMoving = true;
                    isMoving = true;
                    isBreaking = false;
                    break;
                }
            }
            // Update: if the Left Stick passed the test but the right stick did not, then we need to set both as false. 
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
        // This function checks if the sticks (both sticks) are currently in the end position of the STOP movement.
        // It then searches through recent history, to see if both have been in the start position of the STOP movement.
        // If they have, then it turns on the breaks and stops the plane.

        threshold = 40;
        // This compares the current orientation and position with the desired end (sticks crossed above head) 
        float left_angle_stop = Vector3.Angle(LeftStick.transform.localEulerAngles, new Vector3(300, 90, 180));
        float right_angle_stop = Vector3.Angle(RightStick.transform.localEulerAngles, new Vector3(300, 180, 180));
        if ((LeftStick.transform.position.y > 1.7) && (Mathf.Abs(left_angle_stop) < threshold))
        {
            // Now check for the starting position of the STOP motion (arms out to the sides) 
            Vector3 orient;
            for (int i = 0; i < maxHistorySize; i++)
            {
                orient = leftOrientationHistory[maxHistorySize - i - 1];
                left_angle_stop = Vector3.Angle(orient, new Vector3(0, 250, 0));
                if ((leftPositionHistory[i].y < 1.5) && (Mathf.Abs(left_angle_stop) < threshold))
                {
                    LeftStickisMoving = false;
                    break;
                }
            }
        }

        // Only check the Right Stick if the Left Stick is in the correct position 
        if ((!LeftStickisMoving) && (RightStick.transform.position.y > 1.7) && (Mathf.Abs(right_angle_stop) < threshold))
        {
            // Now check for the starting position of the STOP motion
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
            // Update: if the Left Stick passed the test but the right stick did not, then we need to set both as true.
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
        // This function checks that a certain position occurs for the turning to be switched on
        // (left arm bent at the elbow, right arm pointing to the side)

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
        // This function checks that a certain position occurs for the turning to be switched on
        // (right arm bent at the elbow, left arm pointing to the side)
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
        // This function checks that certain criteria are still true while the plane is turning left
        // 1. LeftStick has to be at most 0.1m below the RightStick - to allow for the turning movement
        // 2. LeftStick drops below a certain height (1.4m)

        if ((LeftStick.transform.localPosition.y - RightStick.transform.localPosition.y <= -0.1) || (LeftStick.transform.localPosition.y < 1.4))
        {
            isTurningLeft = false;
        }
    }

    private void PlaneStopTurningRight()
    {
        // This function checks that certain criteria are still true while the plane is turning right
        // 1. RightStick has to be at most 0.1m below the LeftStick - to allow for the turning movement
        // 2. RightStick drops below a certain height (1.4m)
        if ((RightStick.transform.localPosition.y - LeftStick.transform.localPosition.y <= -0.1) || (RightStick.transform.localPosition.y < 1.4))
        {
            isTurningRight = false;
        }
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


        // Are sticks grabbed?
        Stick stickNetworking1 = Marshal1.GetComponent<Stick>();
        Stick stickNetworking2 = Marshal2.GetComponent<Stick>();
        if (stickNetworking1.isGrabbed && stickNetworking2.isGrabbed)
        {
            sticksInHands = true;
        }
        else
        {
            sticksInHands = false;
        }
        // Only want to do one thing per time step!
        // Hence ELSE.

        if (isRunning && sticksInHands) {
            // Priority if stopped is GO
            if (!isMoving)
            {
                PlaneMoving();
            }
            // Priority if moving is STOP
            else if (isMoving && (!isTurningLeft) && (!isTurningRight))
            {
                PlaneStopping();

                // if not STOP but moving, check for turns
                if (!isBreaking)
                {
                    PlaneTurningRight();
                    // if it is not turning right, then check for turning left
                    if (!isTurningRight)
                    {
                        PlaneTurningLeft();
                    }
                }
            }
            // Priority if is is moving and turning is stop turning
            else if (isMoving && isTurningRight)
            {
                PlaneStopTurningRight();
            }
            else if (isMoving && isTurningLeft)
            {
                PlaneStopTurningLeft();
            }
        }

        else if ((!sticksInHands && isMoving) || (isMoving && !isRunning))
        {
            isMoving = false;
            isBreaking = true;
        }
        

        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        SpeedLimit();
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



    public void AttemptStartStop()
    {
        if (isOwner)
        {
            // If the user is the owner, reset directly.
            if(isRunning){
                Debug.Log("Started by server.");
            }
            else{
                Debug.Log("Stopped by server.");
            }

            StartStopObject(isRunning);
            
        }
        else
        {
            if(isRunning){
                Debug.Log("Started by client.");
            }
            else{
                Debug.Log("Stopped by client.");
            }

            // If the user is not the owner, send a reset message to the owner.
            StartMessage msg = new StartMessage();
            msg.isRunning = isRunning;
            context.SendJson(msg);

            Debug.Log("Reset send by user.");

        }
    }

    private struct StartMessage {  
        public bool isRunning; 
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage m)
    {
        if (isOwner)
        {
            Debug.Log("Reset message received by owner.");
            var message = m.FromJson<StartMessage>();
            // Only the owner should listen for reset messages and perform the reset.
            StartStopObject(message.isRunning);
        }
        else
        {
            Debug.Log("Reset message received by non-owner.");
        }    
    }

    public void StartStopObject(bool currentState)
    {
        isRunning = !currentState;
    }

}

