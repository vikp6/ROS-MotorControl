using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using Unity.VisualScripting;

public class RosDynamixelMotorControlBasic : MonoBehaviour
{
    //Motor Control topic name
    private string topicName = "set_position";

    //Set Scene's ROS Manager
    [SerializeField] private RosManager rosmanager;
    
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
        testCube = gameObject;
        testCubestartX = testCube.GameObject().transform.position.x;
    }

    private void Update()
    {
        publishRosMessage();
    }

    public void publishRosMessage()
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
            
            // Finally send the message to server_endpoint.py running in ROS
            rosmanager.publishMotorPos(motorID, motorPosition);

            timeElapsed = 0;
        }
    }
}
