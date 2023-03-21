using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MotorSlider : MonoBehaviour
{

    //Set Scene's ROS Manager
    [SerializeField] private RosManager rosmanager;

    //User can set the number of motors daisy chained
    private static int numbofMotors = 6;
    
    //Saves the most recent position value of a motor before dropdown selection is changed
    private int[] motorSaveState = new int[numbofMotors];

    private int m_MotorPosition = 0;

    private int m_MotorID = 1;

    [SerializeField]
    private Slider m_Slider;

    private Dropdown m_Dropdown;

    private TextMeshPro[] m_PositionText;

    [SerializeField] private GameObject motorDial;

    [SerializeField] private GameObject rightHand;

    [SerializeField] private InputActionReference rightPrimaryButton;
    
    // private bool resetMotors = false;
    
    void Start()
    {

        m_PositionText = gameObject.GetComponentsInChildren<TextMeshPro>();
    }

    private void Awake()
    {
        rightPrimaryButton.action.started += DialControl;
    }

    private void OnDestroy()
    {
        rightPrimaryButton.action.started -= DialControl;
    }

    private void Update()
    {

        //ENABLE THIS FOR HAND CONTROL
        //handTurnMotorControl();
        
        //sendSliderPosition();
        rosmanager.publishMotorPos(m_MotorID, m_MotorPosition);
        
        rosmanager.receiveMotorPos(m_MotorID);
        if (rosmanager.actualPosition <= 1023)
        {
            m_PositionText[1].text = $"A Position: {rosmanager.actualPosition}";
                
        }

    }

    private void DialControl(InputAction.CallbackContext callbackContext)
    {
        print("hello");
        if (motorDial.activeSelf)
        {
            motorDial.SetActive(false);
        }
        else
        {
            motorDial.SetActive(true);

            var handposition = rightHand.transform.position;
            motorDial.transform.position = new Vector3(handposition.x, handposition.y,
                handposition.z + 0.2f);
            
        }
    }

    public void SetPosition(float position)
    {
        m_MotorPosition = (int)position;
        //Debug.Log($"Motor Position: {m_MotorPosition}");
        //sendSliderPosition();
        m_PositionText[0].text = $"E Position: {m_MotorPosition}";

        //Adjust Dial
        var motorEuler = motorDial.transform.eulerAngles;
        float rotDeg = (position / 3.41f)-150;
        
        if (rotDeg > 150)
        {
            rotDeg = 150;
        }
        
        //print(rotDeg);
        //print(rotDeg*Mathf.Deg2Rad);

        motorDial.transform.eulerAngles = new Vector3(motorEuler.x, motorEuler.y, rotDeg);

        //motorDial.transform.rotation = new Quaternion(motorRotation.x, motorRotation.y, rotDeg*Mathf.Deg2Rad, motorRotation.w);



    }

    //Right Hand Rotation can control motor with this, need to get it to be enabled by an input action
    public void handTurnMotorControl()
    {
        float handRot = rightHand.transform.eulerAngles.z;

        
        
        if (handRot is > 150 and < 210)
        {
            
        }
        else
        {
            if (handRot >= 210)
            {
                handRot -= 360;
            }
            m_Slider.value = (handRot + 150) * 3.41f;
        }
        
        
    }
    
    public void SetID(int id)
    {
        //Save prev motor state
        motorSaveState[m_MotorID - 1] = m_MotorPosition;
        
        //Update Motor ID
        m_MotorID = id+1;
        
        //Retrieve previous save state motor position
        //m_MotorPosition = motorSaveState[id];
        m_Slider.value = motorSaveState[id];

        //Debug.Log($"Motor ID: {m_MotorID}");
        //m_MotorPosition = rosmanager.actualPosition;

    }

    // public void sendSliderPosition()
    // {
    //     timeElapsed += Time.deltaTime;
    //
    //     if (timeElapsed > publishMessageFrequency)
    //     {
    //         //Slider Logic Here
    //
    //         // Finally send the message to server_endpoint.py running in ROS
    //         rosmanager.publishMotorPos(m_MotorID, m_MotorPosition);
    //         
    //         m_PositionText[0].text = $"E Position: {m_MotorPosition}";
    //
    //         
    //         timeElapsed = 0;
    //     }
    // }

    // public void getCurrMotorPosition(int MotorID)
    // {
    //     timeElapsed2 += Time.deltaTime;
    //     
    //     float threshold = 0.001f;
    //
    //     if (timeElapsed2 > threshold)
    //     {
    //         rosmanager.receiveMotorPos(MotorID);
    //         if (rosmanager.actualPosition <= 1023)
    //         {
    //             m_PositionText[1].text = $"A Position: {rosmanager.actualPosition}";
    //             
    //         }
    //         
    //         
    //         timeElapsed2 = 0;
    //     }
    // }
}