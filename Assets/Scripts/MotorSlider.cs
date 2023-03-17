using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using TMPro;
using Unity.VisualScripting;
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

    // private bool resetMotors = false;
    
    void Start()
    {

        //Reset all motor positions to 0 

        m_PositionText = gameObject.GetComponentsInChildren<TextMeshPro>();
    }

    private void Update()
    {
        // if (!resetMotors)
        // {
        //     SetID(0);
        //     SetID(1);
        //     SetID(2);
        //     SetID(3);
        //     SetID(4);
        //     SetID(5);
        //     resetMotors = true;
        // }
        
        
        //sendSliderPosition();
        rosmanager.publishMotorPos(m_MotorID, m_MotorPosition);
        
        rosmanager.receiveMotorPos(m_MotorID);
        if (rosmanager.actualPosition <= 1023)
        {
            m_PositionText[1].text = $"A Position: {rosmanager.actualPosition}";
                
        }

    }
    
    

    public void SetPosition(float position)
    {
        m_MotorPosition = (int)position;
        //Debug.Log($"Motor Position: {m_MotorPosition}");
        //sendSliderPosition();
        m_PositionText[0].text = $"E Position: {m_MotorPosition}";
        

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