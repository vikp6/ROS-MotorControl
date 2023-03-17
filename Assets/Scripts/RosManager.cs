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
    
    public string serviceName = "get_position";
    public int actualPosition;
    
    //float awaitingResponseUntilTimestamp = -1;
    
    // Publish the cube's position and rotation every N seconds
    [SerializeField]
    private float publishMessageFrequency = 0.01f;
    
    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    
    private float timeElapsed2;
    
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
    
    public void receiveMotorPos(int motorID)
    {
        timeElapsed2 += Time.deltaTime;
        
        float threshold = 0.1f;

        if (timeElapsed2 > threshold)
        {
            MotorDataMsg motorPos = new MotorDataMsg(
                motorID, 0
            );

            GetPositionRequest getpositionRequest = new GetPositionRequest(motorPos);
            ros.SendServiceMessage<GetPositionResponse>(serviceName, getpositionRequest, Callback_Destination);

            timeElapsed2 = 0;
        }
        


    }
    
    void Callback_Destination(GetPositionResponse response)
    {
        //awaitingResponseUntilTimestamp = -1;
        actualPosition = response.output.position;
        Debug.Log($"Motor Position Service Response: {response.output.position}");
    }
}
