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
        
        MotorDataMsg motorPos = new MotorDataMsg(
            motorID, motorPosition
        );
        
        ros.Publish("set_position", motorPos);
        //Debug.Log($"Published: Motor Position- {motorPosition}, Motor ID- {motorID}");        

    }
    
    public void receiveMotorPos(int motorID)
    {
        MotorDataMsg motorPos = new MotorDataMsg(
            motorID, 0
        );

        GetPositionRequest getpositionRequest = new GetPositionRequest(motorPos);
        ros.SendServiceMessage<GetPositionResponse>(serviceName, getpositionRequest, Callback_Destination);
        //awaitingResponseUntilTimestamp = Time.time + 1.0f;
        //Debug.Log(MotorDataMessage);
        //Add what to do with service data from ROS

    }
    
    void Callback_Destination(GetPositionResponse response)
    {
        //awaitingResponseUntilTimestamp = -1;
        actualPosition = response.output.position;
        Debug.Log($"Motor Position Service Response: {response.output.position}");
    }
}
