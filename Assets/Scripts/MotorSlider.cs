using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class MotorSlider : MonoBehaviour
{

    //Set Scene's ROS Manager
    [SerializeField] private RosManager m_Rosmanager;

    //User can set the number of motors daisy chained
    private static int m_NumbofMotors = 6;
    
    //Saves the most recent position value of a motor before dropdown selection is changed
    private int[] m_MotorSaveState = new int[m_NumbofMotors];

    private int m_MotorPosition = 0;

    private int m_MotorID = 1;

    [SerializeField]
    private Slider m_Slider;

    private Dropdown m_Dropdown;

    private TextMeshPro[] m_PositionText;

    [SerializeField] private GameObject m_MotorDisplay;

    [SerializeField] private GameObject m_MotorDialControl;

    [SerializeField] private GameObject m_RightHand;

    [SerializeField] private InputActionReference m_RightPrimaryButton;
    
    // private bool resetMotors = false;
    
    void Start()
    {

        m_PositionText = gameObject.GetComponentsInChildren<TextMeshPro>();
        m_RightPrimaryButton.action.Enable();
        m_RightPrimaryButton.action.started += DialControl;
        
        RosManager.onPositionReceived += OnPositionReceived;
    }
    

    private void OnDestroy()
    {
        m_RightPrimaryButton.action.started -= DialControl;
    }

    private void Update()
    {

        //ENABLE THIS FOR HAND CONTROL
        //handTurnMotorControl();
        
        //Dial Control
        if (m_MotorDialControl.activeSelf) DialSetPosition();
        
        m_Rosmanager.PublishMotorPos(m_MotorID, m_MotorPosition);
        
        m_Rosmanager.QueryMotorPosition(m_MotorID);


        //rosmanager.QueryMotorPosition(m_MotorID);


    }

    private void OnPositionReceived(int receivedPosition)
    {

        m_PositionText[1].text = $"A Position: {receivedPosition}";
        
        //Adjust Display
        var motorEuler = m_MotorDisplay.transform.eulerAngles;
        float rotDeg = (receivedPosition / 3.41f)-150;
        
        if (rotDeg > 150)
        {
            rotDeg = 150;
        }

        m_MotorDisplay.transform.eulerAngles = new Vector3(motorEuler.x, motorEuler.y, rotDeg);
        
    }
    
    
    private void DialControl(InputAction.CallbackContext callbackContext)
    {
        
        if (m_MotorDialControl.activeSelf)
        {
            m_MotorDialControl.SetActive(false);
            //rightHand.GetComponent<XRRayInteractor>().enabled = true;
            m_RightHand.GetComponentInChildren<XRRayInteractor>().enabled = true;
        }
        else
        {
            m_MotorDialControl.transform.rotation = m_MotorDisplay.transform.rotation;
            
            m_MotorDialControl.SetActive(true);
            //rightHand.GetComponent<XRRayInteractor>().enabled = false;
            m_RightHand.GetComponentInChildren<XRRayInteractor>().enabled = false;



            var handposition = m_RightHand.transform.position;
            m_MotorDialControl.transform.position = new Vector3(handposition.x, handposition.y,
                handposition.z + 0.25f);
            
        }
    }

    public void DialSetPosition()
    {
        float motorEulerZ = m_MotorDialControl.transform.eulerAngles.z;
        print(motorEulerZ);
        
        
        if (motorEulerZ is > 150 and < 210)
        {
            
        }
        else
        {
            if (motorEulerZ >= 210)
            {
                motorEulerZ -= 360;
            }
            
            float sliderPos = (motorEulerZ + 150) * 3.41f;
            if (sliderPos > 1023)
            {
                sliderPos = 1023;
            }
        
            print(sliderPos);

            m_Slider.value = sliderPos;
        }

    }

    public void SetPosition(float position)
    {
        m_MotorPosition = (int)position;
        
        
        m_PositionText[0].text = $"E Position: {m_MotorPosition}";

        // //Adjust Dial
        // var motorEuler = m_MotorDisplay.transform.eulerAngles;
        // float rotDeg = (position / 3.41f)-150;
        //
        // if (rotDeg > 150)
        // {
        //     rotDeg = 150;
        // }
        //
        // m_MotorDisplay.transform.eulerAngles = new Vector3(motorEuler.x, motorEuler.y, rotDeg);

        //Send Haptic
        float value = m_MotorPosition / 1023f;
        m_RightHand.GetComponent<XRBaseController>().SendHapticImpulse(value,.1f);

    }
    
    

    //Right Hand Rotation can control motor with this, need to get it to be enabled by an input action
    // public void handTurnMotorControl()
    // {
    //     float handRot = rightHand.transform.eulerAngles.z;
    //
    //     
    //     
    //     if (handRot is > 150 and < 210)
    //     {
    //         
    //     }
    //     else
    //     {
    //         if (handRot >= 210)
    //         {
    //             handRot -= 360;
    //         }
    //         m_Slider.value = (handRot + 150) * 3.41f;
    //     }
    //     
    //     
    // }
    
    public void SetID(int id)
    {
        //Disable dial control if active
        if (m_MotorDialControl.activeSelf)
        {
            m_MotorDialControl.SetActive(false);
            m_RightHand.GetComponent<XRRayInteractor>().enabled = true;
        }
        
        //Save prev motor state
        m_MotorSaveState[m_MotorID - 1] = m_MotorPosition;
        
        //Update Motor ID
        m_MotorID = id+1;
        
        //Retrieve previous save state motor position
        //m_MotorPosition = motorSaveState[id];
        m_Slider.value = m_MotorSaveState[id];

        //Debug.Log($"Motor ID: {m_MotorID}");
        //m_MotorPosition = rosmanager.actualPosition;

    }
    
}