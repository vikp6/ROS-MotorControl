using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class InputManager : MonoBehaviour
{
    
    [SerializeField]
    private InputActionProperty m_ResetMotorPositions;

    [SerializeField]
    private InputActionProperty m_ToggleMotors;

    [SerializeField] 
    private InputActionProperty m_RotateMotors_Joystick;
    
    [SerializeField] 
    private InputActionProperty m_RotateMotors_Grip;

    [SerializeField] 
    private InputActionProperty m_CalibrateArea;
    
    [SerializeField] 
    private MotorSlider m_MotorController;
    
    [SerializeField] 
    private MotorData m_MotorDataScriptableObject;

    [SerializeField] 
    private GameObject m_LeftHand;

    [SerializeField] 
    private GameObject m_RightHand;

    [SerializeField]
    private LineRenderer m_CalibrationVisualLine;

    [SerializeField] 
    private GameObject m_CalibrationCube;

    private int JoystickFactor = 20;

    private Vector3 m_RightGripStartPosition;
    private Vector3 m_LeftGripStartPosition;
    private int m_MotorCurrStartPos;

    private int m_InteractionFactor = 200;

    private Vector3 m_CalibrationStartPoint;
    private Vector3 m_CalibrationDrag;

    private float m_RealCubeDim = 78;
    
    // Start is called before the first frame update
    void Start()
    {
        EnableInputActions();
        SetupCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void EnableInputActions()
    {
        m_ResetMotorPositions.reference.action.Enable();
        m_ToggleMotors.reference.action.Enable();
        m_RotateMotors_Joystick.reference.action.Enable();
        m_RotateMotors_Grip.reference.action.Enable();
        m_CalibrateArea.reference.action.Enable();
    }
    
    private void SetupCallbacks()
    {
        WheelSelector.onMotorWheelSelected += OnMotorWheelSelected;
        
        m_ResetMotorPositions.reference.action.started += _ => ResetMotorState();
        //m_ToggleMotors.reference.action.started += _ => ToggleMotors();
        m_RotateMotors_Joystick.reference.action.performed += ctx => MotorPosChangeJoystick(ctx);
        m_RotateMotors_Grip.reference.action.started += ctx =>
        {
            m_RightGripStartPosition = ctx.ReadValue<Vector3>();
            m_LeftGripStartPosition = m_LeftHand.transform.position;

            m_MotorCurrStartPos = m_MotorController.MotorPosition;
        };
        m_RotateMotors_Grip.reference.action.performed += ctx => MotorPosChangeGrip(ctx);
        
        //Calibrate Input Action
        m_CalibrateArea.reference.action.started += ctx =>
        {
            
            m_RightHand.GetComponentInChildren<XRRayInteractor>().enabled = false;
            
            m_CalibrationStartPoint = m_RightHand.transform.position;
            m_CalibrationStartPoint += m_RightHand.transform.forward*0.05f;

            m_CalibrationVisualLine.enabled = true;

            m_CalibrationVisualLine.startWidth = 0.01f;
            m_CalibrationVisualLine.endWidth = 0.01f;
            m_CalibrationVisualLine.SetPosition(0, m_CalibrationStartPoint);
            
            

        };
        m_CalibrateArea.reference.action.performed += ctx =>
        {
            m_CalibrationDrag = m_RightHand.transform.position;
            m_CalibrationDrag += m_RightHand.transform.forward*0.05f;
            
            
            
            //Set Calibration Visualizer position to be at drag position
            m_CalibrationVisualLine.SetPosition(1, m_CalibrationDrag);

        };
        m_CalibrateArea.reference.action.canceled += ctx => CalibrationVisuals(ctx);

    }
    
    //Visual Pointers to each of the Motors will be Created Here based on Calibration
    private void CalibrationVisuals(InputAction.CallbackContext ctx)
    {
        Vector3 finalDragPosition = m_CalibrationDrag;
        
        m_RightHand.GetComponentInChildren<XRRayInteractor>().enabled = true;
        m_CalibrationVisualLine.enabled = false;
        m_CalibrationCube.SetActive(true);

        float distance = Vector3.Distance(m_CalibrationStartPoint, finalDragPosition);
        
        m_CalibrationCube.transform.localScale = new Vector3(distance/Mathf.Sqrt(2), distance/Mathf.Sqrt(2), distance/Mathf.Sqrt(2));
        m_CalibrationCube.transform.position = (m_CalibrationStartPoint + finalDragPosition) / 2;

        m_CalibrationCube.transform.up = finalDragPosition - m_CalibrationStartPoint;
        
        m_CalibrationCube.transform.rotation *= Quaternion.AngleAxis(135f, m_CalibrationCube.transform.forward);
        //m_CalibrationCube.transform.rotation *= Quaternion.AngleAxis(180f, m_CalibrationCube.transform.right);
        
        m_CalibrationCube.transform.position += m_CalibrationCube.transform.forward*((distance/Mathf.Sqrt(2))/2);

    }
    
    //Motor Select from Wheel Select, Callback
    private void OnMotorWheelSelected(int newID, Color filterColor)
    {
        Debug.Log($"newID: {newID}");
        
        m_MotorController.SetIDExternal(newID);
    }

    private void MotorPosChangeJoystick(InputAction.CallbackContext ctx)
    {
        Vector2 joystickVec = ctx.ReadValue<Vector2>();
        
        Debug.Log($"joyx: {joystickVec.x}, joyy: {joystickVec.y}");

        int newPosition = (int)(m_MotorController.MotorPosition + (joystickVec.x)*JoystickFactor);
        
        m_MotorController.ChangePositionExternal(newPosition);
    }

    private void MotorPosChangeGrip(InputAction.CallbackContext ctx)
    {
        Vector3 currentRightGripVec = ctx.ReadValue<Vector3>();
        Vector3 currentLeftGripVec = m_LeftHand.transform.position;

        float rightDelta = currentRightGripVec.y - m_RightGripStartPosition.y;
        float leftDelta = currentLeftGripVec.y - m_LeftGripStartPosition.y;

        //Using 2 different interaction methods
        
        if (m_MotorController.MotorID > 2)
        {
            //Debug.Log($"Original X: {m_RightGripStartPosition.x}, Current X: {currentRightGripVec.x}");

            float delta = currentRightGripVec.x - m_RightGripStartPosition.x;

            int newPosition = (int)(m_MotorCurrStartPos + delta * m_InteractionFactor);
        
            m_MotorController.ChangePositionExternal(newPosition);
        }
        else
        {
            float changeFactor = rightDelta - leftDelta;
            // if (changeFactor < 0.2 & changeFactor > -0.2)
            // {
            //     changeFactor = 0;
            // }
            // else
            // {
            //     changeFactor = changeFactor * 5;
            // }
            
            int newPosition = (int)(m_MotorCurrStartPos + changeFactor*m_InteractionFactor);
            Debug.Log($"ChangeFactor: {newPosition}");
            
            m_MotorController.ChangePositionExternal(newPosition);
        }


    }

    private void ResetMotorState()
    {
        // int x = 0;
        // bool setter = false;
        //
        // float m_TimeElapsed = 0;
        // while (x < m_MotorDataScriptableObject.numberOfMotors)
        // {
        //     if (setter == false)
        //     {
        //         m_MotorController.SetID(x);
        //         m_MotorController.ChangePositionExternal(m_MotorDataScriptableObject.MotorPositionBounds[x].start);
        //         setter = true;
        //     }
        //     
        //     m_TimeElapsed += Time.deltaTime;
        //     if (m_TimeElapsed > 0.01f)
        //     {
        //         x++;
        //         setter = false;
        //         m_TimeElapsed = 0;
        //     }
        // }
        
    }

    // private void ToggleMotors()
    // {
    //
    //     int newID = m_MotorController.MotorID;
    //     if (newID == m_MotorDataScriptableObject.numberOfMotors)
    //     {
    //         newID = 0;
    //     }
    //     
    //     Debug.Log($"newID: {newID}");
    //     
    //     m_MotorController.SetIDExternal(newID);
    // }
    
}
