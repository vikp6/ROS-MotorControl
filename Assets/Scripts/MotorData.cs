using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MotorData", order = 1)]
public class MotorData : ScriptableObject
{
    [SerializeField]
    private int m_NumberOfMotors;
    
    [SerializeField]
    private List<int> m_MotorPositionLow = new List<int>();

    [SerializeField]
    private List<int> m_MotorPositionHigh = new List<int>();
    
    public int numberOfMotors
    {
        get => m_NumberOfMotors;
        set => m_NumberOfMotors = value;
    }

    public List<int> MotorPositionLow
    {
        get => m_MotorPositionLow;
        set => m_MotorPositionLow = value;
    }

    public List<int> MotorPositionHigh
    {
        get => m_MotorPositionHigh;
        set => m_MotorPositionHigh = value;
    }
}