using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using Unity.VisualScripting;

public class RosManager : MonoBehaviour
{
    ROSConnection ros;
    private string[] topics = new[]
    {
        "set_position"
    };
    
    private string serviceName = "get_position";
    
    // Publish the cube's position and rotation every N seconds
    [SerializeField]
    private float publishMessageFrequency = 0.01f;
    
    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    
    private float timeElapsed2;

    //Event used to set the actual position value that MotorSlider can subscribe to when invoked
    public static event Action<int> onPositionReceived;
    
    private void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();

        foreach (var topic in topics)
        {
            ros.RegisterPublisher<MotorDataMsg>(topic);
        }
        
        
        //Setup Service Call
        ros.RegisterRosService<GetPositionRequest, GetPositionResponse>(serviceName);
        
    }

    private void Update()
    {
        //receiveMotorPos(3);
    }

    public void publishMotorPos(int motorID, int motorPosition)
    {
        
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            //Slider Logic Here

            // Finally send the message to server_endpoint.py running in ROS
            MotorDataMsg motorPos = new MotorDataMsg(
                motorID, motorPosition
            );
        
            ros.Publish("set_position", motorPos);
            
            
            timeElapsed = 0;
        }
        

        //Debug.Log($"Published: Motor Position- {motorPosition}, Motor ID- {motorID}");        

    }
    
    // public void QueryMotorPosition(int motorID, float timeThreshold=0.1f)
    // {
    //     timeElapsed2 += Time.deltaTime;
    //     
    //     if (timeElapsed2 > timeThreshold)
    //     {
    //         MotorDataMsg motorPos = new MotorDataMsg(
    //             motorID, 0
    //         );
    //
    //         GetPositionRequest getpositionRequest = new GetPositionRequest(motorPos);
    //         //ros.SendServiceMessage<GetPositionResponse>(serviceName, getpositionRequest, GetMotorPosition);
    //         
    //         ros.SendServiceMessage<GetPositionResponse>(serviceName, getpositionRequest, response => onPositionReceived?.Invoke(response.output.position));
    //
    //         timeElapsed2 = 0;
    //     }
    //     
    // }
    
    public void QueryMotorPosition(int motorID, float timeThreshold=0.1f)
    {
        timeElapsed2 += Time.deltaTime;
        
        if (timeElapsed2 > timeThreshold)
        {
            GetMotorPosition(motorID);
            
            //onPositionReceived?.Invoke(position);
            
            timeElapsed2 = 0;
        }
    
    
    }
    
    private void GetMotorPosition(int motorID)
    {
        MotorDataMsg motorPos = new MotorDataMsg(
            motorID, 0
        );
    
        GetPositionRequest getpositionRequest = new GetPositionRequest(motorPos);
        ros.SendServiceMessage<GetPositionResponse>(serviceName, getpositionRequest, response => onPositionReceived?.Invoke(response.output.position));
        
    }

    // void GetMotorPosition(GetPositionResponse response)
    // {
    //     onPositionReceived?.Invoke(response.output.position);
    //     
    //     Debug.Log($"Motor Position Service Response: {response.output.position}");
    // }
}
