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

    private void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();

        foreach (var topic in topics)
        {
            ros.RegisterPublisher<SetPositionMsg>(topic);
        }
        
        
    }
    
    public void publishMotorPos(string topicName, SetPositionMsg cubePos)
    {
        ros.Publish(topicName, cubePos);
    }
}
