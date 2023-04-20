using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private MotorSlider m_MotorController;
    
    [SerializeField] 
    private MotorData m_MotorDataScriptableObject;

    [SerializeField] 
    private GameObject m_LeftHand;

    private int JoystickFactor = 20;

    private Vector3 m_RightGripStartPosition;
    private Vector3 m_LeftGripStartPosition;
    
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
    }
    
    private void SetupCallbacks()
    {
        m_ResetMotorPositions.reference.action.started += _ => ResetMotorState();
        m_ToggleMotors.reference.action.started += _ => ToggleMotors();
        m_RotateMotors_Joystick.reference.action.performed += ctx => MotorPosChangeJoystick(ctx);
        m_RotateMotors_Grip.reference.action.started += ctx =>
        {
            m_RightGripStartPosition = ctx.ReadValue<Vector3>();
            m_LeftGripStartPosition = m_LeftHand.transform.position;
        };
        m_RotateMotors_Grip.reference.action.performed += ctx => MotorPosChangeGrip(ctx);
        
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

            int newPosition = (int)(m_MotorController.MotorPosition + delta * 10);
        
            m_MotorController.ChangePositionExternal(newPosition);
        }
        else
        {
            float changeFactor = rightDelta - leftDelta;
            if (changeFactor < 0.2 & changeFactor > -0.2)
            {
                changeFactor = 0;
            }
            else
            {
                changeFactor = changeFactor * 5;
            }
            
            int newPosition = (int)(m_MotorController.MotorPosition + changeFactor);
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

    private void ToggleMotors()
    {

        int newID = m_MotorController.MotorID;
        if (newID == m_MotorDataScriptableObject.numberOfMotors)
        {
            newID = 0;
        }
        
        Debug.Log($"newID: {newID}");
        
        m_MotorController.SetIDExternal(newID);
    }
    
}
