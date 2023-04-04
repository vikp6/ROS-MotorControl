using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MotorData", order = 1)]
public class MotorData : ScriptableObject
{
    [SerializeField]
    private int m_NumberOfMotors;

    [SerializeField] 
    private List<MotorPositionBounds> m_MotorPositionBounds = new List<MotorPositionBounds>();

    public int numberOfMotors
    {
        get => m_NumberOfMotors;
        set => m_NumberOfMotors = value;
    }

    public List<MotorPositionBounds> MotorPositionBounds
    {
        get => m_MotorPositionBounds;
        set => m_MotorPositionBounds = value;
    }
}

[System.Serializable]
public struct MotorPositionBounds
{
    public int low;
    public int high;
}