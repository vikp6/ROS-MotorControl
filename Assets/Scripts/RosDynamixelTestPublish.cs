using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using Unity.VisualScripting;

public class RosDynamixelTestPublish : MonoBehaviour
{
    ROSConnection ros;
    private string topicName = "set_position";

    // The game object
    //public GameObject cube;
    // Publish the cube's position and rotation every N seconds
    [SerializeField]
    private float publishMessageFrequency = 0.5f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    [SerializeField]
    private int motorPosition = 0;

    [SerializeField] private int motorID = 1;
    
    private GameObject testCube;

    private float testCubestartX;
    
    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<MotorDataMsg>(topicName);

        testCube = gameObject;
        testCubestartX = testCube.GameObject().transform.position.x;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            //cube.transform.rotation = Random.rotation;

            //Displacement between right hand starting position and current position
            float displacement = Math.Abs(testCubestartX - testCube.GameObject().transform.position.x);
            int motordisplacement = (int)(200 * displacement);
            if (motordisplacement > 1023) motordisplacement = 1023;

            motorPosition = motordisplacement;
            
            MotorDataMsg cubePos = new MotorDataMsg(
                motorID, motordisplacement
            );

            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, cubePos);

            timeElapsed = 0;
        }
    }
}
