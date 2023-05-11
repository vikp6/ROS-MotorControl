using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class MotorSlider : MonoBehaviour
{

    //Set Scene's ROS Manager
    [SerializeField] 
    private RosManager m_Rosmanager;

    //Scriptable Object
    [SerializeField] 
    private MotorData m_MotorDataScriptableObject;

    //Saves the most recent position value of a motor before dropdown selection is changed
    private int[] m_MotorSaveState;
    private int[] m_MotorReset;

    //Motor Position and ID that are constantly updated and published to ROS
    private int m_MotorPosition = 0;
    private int m_MotorID = 1;
    
    public int MotorPosition
    {
        get => m_MotorPosition;
    }
    
    public int MotorID
    {
        get => m_MotorID;
    }

    [SerializeField]
    private Slider m_Slider;

    [SerializeField] 
    private TMP_Dropdown m_Dropdown;

    [SerializeField] 
    private GameObject m_MotorDisplay;

    [SerializeField] 
    private GameObject m_MotorDialControl;

    [SerializeField] 
    private GameObject m_RightHand;

    [SerializeField] 
    private InputActionReference m_RightPrimaryButton;

    [SerializeField] 
    private InputActionReference m_MenuToggleButton;

    private bool m_MenuState = true;
    private Vector3 m_OriginalMenuScale;
    private Vector3 m_OriginalMenuLocalPos;
    
    private TextMeshPro[] m_PositionText;
    
    void Start()
    {
        //Populate savestate array with position low bound values
        m_MotorSaveState = new int[m_MotorDataScriptableObject.numberOfMotors];
        for(int i = 0;i<m_MotorSaveState.Length;i++)
        {
            m_MotorSaveState[i] = m_MotorDataScriptableObject.MotorPositionBounds[i].start;
        }
        
        //Motor 1 set m_MotorPosition to init state
        m_MotorPosition = m_MotorSaveState[0];

        m_PositionText = gameObject.GetComponentsInChildren<TextMeshPro>();
        m_RightPrimaryButton.action.Enable();
        //m_RightPrimaryButton.action.started += DialControl;
        
        m_MenuToggleButton.action.Enable();
        m_MenuToggleButton.action.started += MenuToggle;
        m_OriginalMenuScale = gameObject.transform.localScale;
        m_OriginalMenuLocalPos = gameObject.transform.localPosition;
        
        RosManager.onPositionReceived += OnPositionReceived;
    }
    

    private void OnDestroy()
    {
        m_RightPrimaryButton.action.started -= DialControl;
    }

    private void Update()
    {

        //ENABLE THIS FOR HAND CONTROL
        //handTurnMotorControl();
        
        //Dial Control
        if (m_MotorDialControl.activeSelf) DialSetPosition();
        
        //Publish to ROS
        m_Rosmanager.PublishMotorPos(m_MotorID, m_MotorPosition);
        
        //Query motor position from ROS
        m_Rosmanager.QueryMotorPosition(m_MotorID);

    }

    private void MenuToggle(InputAction.CallbackContext callbackContext)
    {
        if (m_MenuState)
        {
            gameObject.transform.localScale = new Vector3(0.000001f, 0.000001f, 0.000001f);
            //gameObject.transform.localPosition = new Vector3(0, 0, 0);
            
            Debug.Log($"Entered Toggle Off");
            m_MenuState = false;
        }
        else
        {
            gameObject.transform.localScale = m_OriginalMenuScale;
            //gameObject.transform.localPosition = m_OriginalMenuLocalPos;
            m_MenuState = true;
        }
        
    }

    private void OnPositionReceived(int receivedPosition)
    {

        m_PositionText[1].text = $"A Position: {receivedPosition}";
        
        //Adjust Display
        var motorEuler = m_MotorDisplay.transform.eulerAngles;
        float rotDeg = (receivedPosition / 3.41f)-150;
        
        if (rotDeg > 150)
        {
            rotDeg = 150;
        }

        m_MotorDisplay.transform.eulerAngles = new Vector3(motorEuler.x, motorEuler.y, rotDeg);
        
    }
    
    
    private void DialControl(InputAction.CallbackContext callbackContext)
    {
        
        if (m_MotorDialControl.activeSelf)
        {
            m_MotorDialControl.SetActive(false);
            m_RightHand.GetComponentInChildren<XRRayInteractor>().enabled = true;
        }
        else
        {
            m_MotorDialControl.transform.rotation = m_MotorDisplay.transform.rotation;
            
            m_MotorDialControl.SetActive(true);
            m_RightHand.GetComponentInChildren<XRRayInteractor>().enabled = false;



            var handposition = m_RightHand.transform.position;
            m_MotorDialControl.transform.position = new Vector3(handposition.x, handposition.y,
                handposition.z + 0.25f);
            
        }
    }

    public void DialSetPosition()
    {
        float motorEulerZ = m_MotorDialControl.transform.eulerAngles.z;
        print(motorEulerZ);
        
        
        if (motorEulerZ is > 150 and < 210)
        {
            
        }
        else
        {
            if (motorEulerZ >= 210)
            {
                motorEulerZ -= 360;
            }
            
            float sliderPos = (motorEulerZ + 150) * 3.41f;
            if (sliderPos > m_MotorDataScriptableObject.MotorPositionBounds[m_MotorID-1].high)
            {
                sliderPos = m_MotorDataScriptableObject.MotorPositionBounds[m_MotorID-1].high;
            }
            else if (sliderPos < m_MotorDataScriptableObject.MotorPositionBounds[m_MotorID-1].low)
            {
                sliderPos = m_MotorDataScriptableObject.MotorPositionBounds[m_MotorID-1].low;
            }
        
            print(sliderPos);

            m_Slider.value = sliderPos;
        }

    }

    public void SetPosition(float position)
    {
        m_MotorPosition = (int)position;

        m_PositionText[0].text = $"E Position: {m_MotorPosition}";

        //Send Haptic
        float maxPos = m_MotorDataScriptableObject.MotorPositionBounds[m_MotorID-1].high;
        float value = m_MotorPosition / maxPos;
        m_RightHand.GetComponent<XRBaseController>().SendHapticImpulse(value,.1f);

    }

    public void ChangePositionExternal(int position)
    {
        m_Slider.value = position;
    }

    //Right Hand Rotation can control motor with this, need to get it to be enabled by an input action
    // public void handTurnMotorControl()
    // {
    //     float handRot = rightHand.transform.eulerAngles.z;
    //
    //     
    //     
    //     if (handRot is > 150 and < 210)
    //     {
    //         
    //     }
    //     else
    //     {
    //         if (handRot >= 210)
    //         {
    //             handRot -= 360;
    //         }
    //         m_Slider.value = (handRot + 150) * 3.41f;
    //     }
    //     
    //     
    // }
    
    public void SetID(int id)
    {
        //Disable dial control if active
        if (m_MotorDialControl.activeSelf)
        {
            m_MotorDialControl.SetActive(false);
            m_RightHand.GetComponent<XRRayInteractor>().enabled = true;
        }
        
        //Save prev motor state
        m_MotorSaveState[m_MotorID - 1] = m_MotorPosition;
        
        //Update Motor ID
        m_MotorID = id+1;
        
        m_Slider.minValue = m_MotorDataScriptableObject.MotorPositionBounds[id].low;
        m_Slider.maxValue = m_MotorDataScriptableObject.MotorPositionBounds[id].high;
        
        //Retrieve previous save state motor position
        m_Slider.value = m_MotorSaveState[id];

    }

    public void SetIDExternal(int id)
    {
        m_Dropdown.value = id;   
    }
    
}