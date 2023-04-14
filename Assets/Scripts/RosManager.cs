using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class RosManager : MonoBehaviour
{
    ROSConnection m_Ros;
    private string[] m_Topics = new[]
    {
        "set_position"
    };
    
    private string m_ServiceName = "get_position";
    
    // Publish the cube's position and rotation every N seconds
    [SerializeField]
    private float m_PublishMessageFrequency = 0.01f;
    
    // Used to determine how much time has elapsed since the last message was published
    private float m_TimeElapsed;
    
    private float m_TimeElapsed2;

    //Event used to set the actual position value that MotorSlider can subscribe to when invoked
    public static event Action<int> onPositionReceived;
    
    private void Start()
    {
        // start the ROS connection
        m_Ros = ROSConnection.GetOrCreateInstance();

        foreach (var topic in m_Topics)
        {
            m_Ros.RegisterPublisher<MotorDataMsg>(topic);
        }
        
        
        //Setup Service Call
        m_Ros.RegisterRosService<GetPositionRequest, GetPositionResponse>(m_ServiceName);
        
    }

    private void Update()
    {
        //receiveMotorPos(3);
    }

    public void PublishMotorPos(int motorID, int motorPosition)
    {
        
        m_TimeElapsed += Time.deltaTime;

        if (m_TimeElapsed > m_PublishMessageFrequency)
        {
            //Slider Logic Here

            // Finally send the message to server_endpoint.py running in ROS
            MotorDataMsg motorPos = new MotorDataMsg(
                motorID, motorPosition
            );
        
            m_Ros.Publish("set_position", motorPos);
            
            
            m_TimeElapsed = 0;
        }
        

        //Debug.Log($"Published: Motor Position- {motorPosition}, Motor ID- {motorID}");        

    }
    
    public void QueryMotorPosition(int motorID, float timeThreshold=0.1f)
    {
        m_TimeElapsed2 += Time.deltaTime;
        
        if (m_TimeElapsed2 > timeThreshold)
        {
            MotorDataMsg motorPos = new MotorDataMsg(
                motorID, 0
            );

            GetPositionRequest getpositionRequest = new GetPositionRequest(motorPos);
            m_Ros.SendServiceMessage<GetPositionResponse>(m_ServiceName, getpositionRequest, GetMotorPosition);

            m_TimeElapsed2 = 0;
        }
        
    }

    void GetMotorPosition(GetPositionResponse response)
    {
        onPositionReceived?.Invoke(response.output.position);
        
        //Debug.Log($"Motor Position Service Response: {response.output.position}");
    }
}
