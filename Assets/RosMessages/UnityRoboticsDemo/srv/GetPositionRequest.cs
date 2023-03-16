//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.UnityRoboticsDemo
{
    [Serializable]
    public class GetPositionRequest : Message
    {
        public const string k_RosMessageName = "dynamixel_sdk_custom_interfaces/GetPosition";
        public override string RosMessageName => k_RosMessageName;

        public MotorDataMsg input;

        public GetPositionRequest()
        {
            this.input = new MotorDataMsg();
        }

        public GetPositionRequest(MotorDataMsg input)
        {
            this.input = input;
        }

        public static GetPositionRequest Deserialize(MessageDeserializer deserializer) => new GetPositionRequest(deserializer);

        private GetPositionRequest(MessageDeserializer deserializer)
        {
            this.input = MotorDataMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.input);
        }

        public override string ToString()
        {
            return "GetPositionRequest: " +
            "\ninput: " + input.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
