using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using Unity.VisualScripting;

public class RosDynamixelTestPublish : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "set_position";

    // The game object
    public GameObject cube;
    // Publish the cube's position and rotation every N seconds
    public float publishMessageFrequency = 0.5f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    [SerializeField]
    private int motorPosition = 0;

    [SerializeField]
    private GameObject rightHand;

    private float rightHandstartX;
    
    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<SetPositionMsg>(topicName);

        rightHandstartX = rightHand.GameObject().transform.position.x;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            //cube.transform.rotation = Random.rotation;

            //Displacement between right hand starting position and current position
            float displacement = Math.Abs(rightHandstartX - rightHand.GameObject().transform.position.x);
            int motordisplacement = (int)(200 * displacement);
            if (motordisplacement > 1023) motordisplacement = 1023;

            motorPosition = motordisplacement;
            
            SetPositionMsg cubePos = new SetPositionMsg(
                    1,
                    motordisplacement
            );

            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, cubePos);

            timeElapsed = 0;
        }
    }
}
