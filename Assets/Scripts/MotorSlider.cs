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
    
    // Publish the cube's position and rotation every N seconds
    [SerializeField]
    private float publishMessageFrequency = 0.01f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    
    private float timeElapsed2;
    
    private int m_MotorPosition = 0;

    private int m_MotorID = 1;

    private Slider m_Slider;

    private Dropdown m_Dropdown;

    private TextMeshPro[] m_PositionText;
    
    void Start()
    {
        //Slider Listener
        //m_Slider = gameObject.GetComponentInChildren<Slider>();
        
        //mySlider.onValueChanged.AddListener (delegate {sendSliderPosition ();});

        // m_Dropdown = gameObject.GetComponentInChildren<Dropdown>();

        m_PositionText = gameObject.GetComponentsInChildren<TextMeshPro>();
    }

    private void Update()
    {
        sendSliderPosition();
        getSliderPosition();
    }

    public void SetPosition(float position)
    {
        m_MotorPosition = (int)position;
        //Debug.Log($"Motor Position: {m_MotorPosition}");

        

    }
    
    public void SetID(int id)
    {
        m_MotorID = id+1;
        //Debug.Log($"Motor ID: {m_MotorID}");

    }

    public void sendSliderPosition()
    {
        timeElapsed += Time.deltaTime;

        //motorPosition = (int)m_Slider.value;
        //motorID = m_Dropdown.value;
        
        if (timeElapsed > publishMessageFrequency)
        {
            //Slider Logic Here

            // Finally send the message to server_endpoint.py running in ROS
            rosmanager.publishMotorPos(m_MotorID, m_MotorPosition);
            
            m_PositionText[0].text = $"E Position: {m_MotorPosition}";

            
            timeElapsed = 0;
        }
    }

    public void getSliderPosition()
    {
        timeElapsed2 += Time.deltaTime;
        
        if (timeElapsed2 > .1)
        {
            rosmanager.receiveMotorPos(m_MotorID);
            if (rosmanager.actualPosition <= 1023)
            {
                m_PositionText[1].text = $"A Position: {rosmanager.actualPosition}";
            }
            
            timeElapsed2 = 0;
        }
    }
}