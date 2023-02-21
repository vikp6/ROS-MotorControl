using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosColor = RosMessageTypes.UnityRoboticsDemo.UnityColorMsg;
using RosPosRot = RosMessageTypes.UnityRoboticsDemo.PosRotMsg;

public class RosPosRotSubscriber : MonoBehaviour
{
    public GameObject cube;

    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<RosPosRot>("posrot", PosRotChange);
    }

    void PosRotChange(RosPosRot posrotMessage)
    {
        cube.transform.position = new Vector3(posrotMessage.pos_x+2,posrotMessage.pos_y,posrotMessage.pos_z);
        cube.transform.rotation = new Quaternion(posrotMessage.rot_x, posrotMessage.rot_y, posrotMessage.rot_z,
            posrotMessage.rot_w);

    }
}