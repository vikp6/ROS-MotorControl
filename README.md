# ROS-MotorControl

## Unity ROS TCP Setup
To setup the ROS TCP connector on the Unity side, please follow the instructions given in [this](https://github.com/Unity-Technologies/ROS-TCP-Connector) repo.

You will require Unity 2020.2 or later. Once you've added the package, you should be able to add a window within Unity.

## Unity ROS Tutorials
Take a look through the Unity-Robotics-Hub repo and tutorials [here](https://github.com/Unity-Technologies/Unity-Robotics-Hub/tree/main/tutorials/ros_unity_integration).

Start with this [setup](https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/setup.md) and follow the instructions listed for ROS2. I would recommend creating your own colcon workspace (please visit my complement repo [here](https://github.com/vikp6/IXR_ROS_Files)) which details how to setup ROS2 Humble Haskill on a Linux machine) but feel free to try with a docker image.

I would also recommend creating a fresh Unity project to try out these tutorials before cloning this repo.

The [publisher](https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/publisher.md) and [subscriber](https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/subscriber.md) tutorials will be very helpful in understanding how data is sent to and from ROS topics. In a nutshell, a C# script in Unity creates a Message object that contains the relevant data. This Message is then published to a specified ROS topic on the server. The Linux ROS2 node will be subscribed to this node and a callback will be triggered whenever information is published to it.

## Understanding the Code in this Repository
Once you have gone through the tutorials, clone this repository and take a look at these key locations:

1. [ROS Message Files](https://github.com/vikp6/ROS-MotorControl/tree/main/Assets/RosMessages/UnityRoboticsDemo)
This files in the srv and msg folders are used to send and receive the motor ID and position of the Dynamixel motors that are being controlled in this project. Familiarize yourself with the class as you will need to replicate this in case additional sensors/motors and messages need to be added.

2. [ROS Manager Script](https://github.com/vikp6/ROS-MotorControl/blob/main/Assets/Scripts/RosManager.cs)
The ROS Manager script is the bridge between your Unity development and the ROS messaging protocols. This script includes two public functions ```PublishMotorPos``` and ```QueryMotorPosition``` to send and receive the motor data respectively. Similarly, understanding understanding this script will enable replication of it for any sensor/motor extensions.
