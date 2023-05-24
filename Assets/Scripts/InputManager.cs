using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
    private InputActionProperty m_RightPrimaryButton;
    
    [SerializeField] 
    private MotorSlider m_MotorController;
    
    [SerializeField] 
    private MotorData m_MotorDataScriptableObject;

    [SerializeField] 
    private GameObject m_LeftHand;

    [SerializeField] 
    private GameObject m_RightHand;

    [SerializeField] 
    private GameObject m_XROrigin;

    [SerializeField]
    private LineRenderer m_CalibrationVisualLine;

    [SerializeField] 
    private GameObject m_CalibrationCube;

    [SerializeField] 
    private MeshRenderer m_Arrow1;
    
    [SerializeField] 
    private MeshRenderer m_Arrow2;

    [SerializeField] 
    private MeshRenderer m_Arrow3;
    
    [SerializeField] 
    private MeshRenderer m_Arrow4;
    
    [SerializeField] 
    private GameObject m_InteractionVisual;
    
    

    private int JoystickFactor = 15;

    private Vector3 m_RightGripStartPosition;
    private Vector3 m_LeftGripStartPosition;
    private int m_MotorCurrStartPos;

    //Interaction
    private int m_InteractionFactor = 200;
    private Vector3 startInteractionMidpoint;
    private Vector3 calculatedZeroDegVec_HorizontalSteer;
    private Vector3 calculatedZeroDegVec_VerticalSteer;

    private int twoDOFToggleThresh = 25;
    

    private Vector3 m_CalibrationStartPoint;
    private Vector3 m_CalibrationDrag;

    private float m_RealCubeDim = 78;

    private int m_SavedID;
    
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
        m_RightPrimaryButton.reference.action.Enable();
    }
    
    private void SetupCallbacks()
    {
        WheelSelector.onMotorWheelSelected += OnMotorWheelSelected;
        
        m_ResetMotorPositions.reference.action.started += _ => ResetMotorState();
        //m_ToggleMotors.reference.action.started += _ => ToggleMotors();
        m_RotateMotors_Joystick.reference.action.performed += ctx => MotorPosChangeJoystick(ctx);
        m_RotateMotors_Grip.reference.action.started += ctx =>
        {
            m_RightGripStartPosition = m_RightHand.transform.position;
            m_LeftGripStartPosition = m_LeftHand.transform.position;
            
            m_RightHand.GetComponentInChildren<XRRayInteractor>().enabled = false;
            m_LeftHand.GetComponentInChildren<XRRayInteractor>().enabled = false;

            m_MotorCurrStartPos = m_MotorController.MotorPosition;
            
            //Steering Wheel Interaction
            startInteractionMidpoint = (m_RightGripStartPosition + m_LeftGripStartPosition) / 2;
            
            m_InteractionVisual.SetActive(true);
            m_InteractionVisual.transform.position = startInteractionMidpoint;

            float radius = Vector3.Distance(m_RightGripStartPosition, startInteractionMidpoint);

            //Horizontal Steer
            calculatedZeroDegVec_HorizontalSteer = Vector3.Cross(m_LeftGripStartPosition - m_RightGripStartPosition, Vector3.up).normalized * radius;
            
            //Vertical Steer
            calculatedZeroDegVec_VerticalSteer = Vector3.Cross(m_LeftGripStartPosition - m_RightGripStartPosition, m_XROrigin.transform.forward*-1).normalized * radius;

            
        };
        m_RotateMotors_Grip.reference.action.performed += ctx => MotorPosChangeGrip(ctx);
        m_RotateMotors_Grip.reference.action.canceled += ctx =>
        {
            m_InteractionVisual.SetActive(false);
            
            m_RightHand.GetComponentInChildren<XRRayInteractor>().enabled = true;
            m_LeftHand.GetComponentInChildren<XRRayInteractor>().enabled = true;
        };
        
        //Toggle Within Clump Obstacles
        m_RightPrimaryButton.reference.action.started += _ =>
        {
            m_SavedID = m_MotorController.MotorID - 1;
            m_MotorController.SetID(100);
            
            // m_RightGripStartPosition = m_RightHand.transform.position;
            // m_LeftGripStartPosition = m_LeftHand.transform.position;
            //
            // if(m_MotorController.MotorID==1) m_MotorController.SetIDExternal(2);
            // else if(m_MotorController.MotorID==4) m_MotorController.SetIDExternal(4);
            // else if(m_MotorController.MotorID==3) m_MotorController.SetIDExternal(0);
            // else if(m_MotorController.MotorID==5) m_MotorController.SetIDExternal(3);
            //
            // //RESET WHEEL INTERACTION PARAMETERS
            // m_MotorCurrStartPos = m_MotorController.MotorPosition;
            //
            // //Steering Wheel Interaction
            // startInteractionMidpoint = (m_RightGripStartPosition + m_LeftGripStartPosition) / 2;
            //
            // m_InteractionVisual.transform.position = startInteractionMidpoint;
            //
            // float radius = Vector3.Distance(m_RightGripStartPosition, startInteractionMidpoint);
            //
            // //Horizontal Steer
            // calculatedZeroDegVec_HorizontalSteer = Vector3.Cross(m_LeftGripStartPosition - m_RightGripStartPosition, Vector3.up).normalized * radius;
            //
            // //Vertical Steer
            // calculatedZeroDegVec_VerticalSteer = Vector3.Cross(m_LeftGripStartPosition - m_RightGripStartPosition, m_XROrigin.transform.forward*-1).normalized * radius;
            
            
        };
        m_RightPrimaryButton.reference.action.canceled += _ =>
        {
            
            m_MotorController.SetID(m_SavedID);
            
        };
        
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

        switch (newID)
        {
            case 0:
                m_Arrow1.material.color = Color.HSVToRGB(120f / 360f, 1, 1);
                m_Arrow1.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, 1, 1));
                
                m_Arrow2.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow2.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_Arrow3.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow3.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_Arrow4.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow4.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_MotorController.SetIDExternal(5);
                break;
            
            case 1:
                m_Arrow4.material.color = Color.HSVToRGB(120f / 360f, 1, 1);
                m_Arrow4.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, 1, 1));
                
                m_Arrow2.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow2.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_Arrow3.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow3.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_Arrow1.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow1.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_MotorController.SetIDExternal(0);
                break;
                
            case 2:
                m_Arrow3.material.color = Color.HSVToRGB(120f / 360f, 1, 1);
                m_Arrow3.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, 1, 1));
                
                m_Arrow2.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow2.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_Arrow1.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow1.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_Arrow4.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow4.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_MotorController.SetIDExternal(1);
                break;
            
            case 3:
                m_Arrow2.material.color = Color.HSVToRGB(120f / 360f, 1, 1);
                m_Arrow2.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, 1, 1));
                
                m_Arrow1.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow1.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_Arrow3.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow3.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_Arrow4.material.color = Color.HSVToRGB(120f / 360f, .5f, .5f);
                m_Arrow4.material.SetColor("_EmissionColor", Color.HSVToRGB(120f / 360f, .5f, .5f));
                
                m_MotorController.SetIDExternal(3);
                break;
        }
        
        
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
        Vector3 currentRightGripVec = m_RightHand.transform.position;
        Vector3 currentLeftGripVec = m_LeftHand.transform.position;
        
        // //Up Down Interaction
        // float rightDelta = currentRightGripVec.y - m_RightGripStartPosition.y;
        // float leftDelta = currentLeftGripVec.y - m_LeftGripStartPosition.y;
        
        // //Steering Wheel Interaction
        // Vector3 startMidpoint = (m_RightGripStartPosition + m_LeftGripStartPosition) / 2;
        //
        // m_InteractionVisual.transform.position = startMidpoint;
        //
        // float radius = Vector3.Distance(m_RightGripStartPosition, startMidpoint);
        //
        // //Horizontal Steer
        // Vector3 calculatedZeroDegVec_HorizontalSteer = Vector3.Cross(m_LeftGripStartPosition - m_RightGripStartPosition, Vector3.up).normalized * radius;
        
        
            
        Vector3 vecRH = currentRightGripVec - startInteractionMidpoint;
        Vector3 vecLH = currentLeftGripVec - startInteractionMidpoint;
            
        float angleRH = Vector3.Angle(vecRH, calculatedZeroDegVec_HorizontalSteer);
        float angleLH = Vector3.Angle(vecLH, calculatedZeroDegVec_HorizontalSteer);
        
        //m_InteractionVisual.GetComponentInChildren<TextMeshProUGUI>().text = "R: "+angleRH+"\n"+"L: "+angleLH;

        // //Vertical Steer
        // Vector3 calculatedZeroDegVec_VerticalSteer = Vector3.Cross(m_LeftGripStartPosition - m_RightGripStartPosition, m_XROrigin.transform.forward*-1).normalized * radius;

        Vector3 vecRV = currentRightGripVec - startInteractionMidpoint;
        Vector3 vecLV = currentLeftGripVec - startInteractionMidpoint;
            
        float angleRV = Vector3.Angle(vecRV, calculatedZeroDegVec_VerticalSteer);
        float angleLV = Vector3.Angle(vecLV, calculatedZeroDegVec_VerticalSteer);
        
        
        
        if (m_MotorController.MotorID==1 | m_MotorController.MotorID==4)
        {
            //m_InteractionVisual.transform.forward = -1*calculatedZeroDegVec_HorizontalSteer;
            m_InteractionVisual.transform.eulerAngles = new Vector3(0,
                m_InteractionVisual.transform.eulerAngles.y, m_InteractionVisual.transform.eulerAngles.z);
            
            //Horizontal Steer
            float constant = 2;
            if (m_MotorController.MotorID == 1) constant = 2.5f;
            
            int newPosition = (int)(m_MotorCurrStartPos+(angleRH-angleLH)*constant);

            m_MotorController.ChangePositionExternal(newPosition);


            if (angleRV - angleLV > twoDOFToggleThresh | angleRV - angleLV < -twoDOFToggleThresh)
            {
                if(m_MotorController.MotorID==1) m_MotorController.SetIDExternal(2);
                if(m_MotorController.MotorID==4) m_MotorController.SetIDExternal(4);
                
                //RESET WHEEL INTERACTION PARAMETERS
                m_MotorCurrStartPos = m_MotorController.MotorPosition;
            
                //Steering Wheel Interaction
                startInteractionMidpoint = (currentRightGripVec + currentLeftGripVec) / 2;

                //m_InteractionVisual.transform.position = startInteractionMidpoint;

                float radius = Vector3.Distance(currentRightGripVec, startInteractionMidpoint);

                //Vertical Steer
                calculatedZeroDegVec_VerticalSteer = Vector3.Cross(currentLeftGripVec - currentRightGripVec, m_XROrigin.transform.forward*-1).normalized * radius;
                
                //m_InteractionVisual.transform.forward = -1*calculatedZeroDegVec_VerticalSteer;

            }
            
            
            
        }
        else if (m_MotorController.MotorID==3 | m_MotorController.MotorID==5)
        {

            //m_InteractionVisual.transform.forward = -1*calculatedZeroDegVec_VerticalSteer;
            
            m_InteractionVisual.transform.eulerAngles = new Vector3(-90,
                m_InteractionVisual.transform.eulerAngles.y, m_InteractionVisual.transform.eulerAngles.z);
            
            //Vertical Steer
            float constantV = -1f;
            if (m_MotorController.MotorID == 3) constantV = 1.5f;
            
            int newPositionV = (int)(m_MotorCurrStartPos+(angleRV-angleLV)*constantV);

            m_MotorController.ChangePositionExternal(newPositionV);

            if (angleRH - angleLH > twoDOFToggleThresh | angleRH - angleLH < -twoDOFToggleThresh)
            {
                if(m_MotorController.MotorID==3) m_MotorController.SetIDExternal(0);
                if(m_MotorController.MotorID==5) m_MotorController.SetIDExternal(3);
                
                //RESET WHEEL INTERACTION PARAMETERS
                m_MotorCurrStartPos = m_MotorController.MotorPosition;
            
                //Steering Wheel Interaction
                startInteractionMidpoint = (currentRightGripVec + currentLeftGripVec) / 2;

                //m_InteractionVisual.transform.position = startInteractionMidpoint;

                float radius = Vector3.Distance(currentRightGripVec, startInteractionMidpoint);

                //Horizontal Steer
                calculatedZeroDegVec_HorizontalSteer = Vector3.Cross(currentLeftGripVec - currentRightGripVec, Vector3.up).normalized * radius;
                
                //m_InteractionVisual.transform.forward = -1*calculatedZeroDegVec_HorizontalSteer;

            }
            
            
        }
        else if(m_MotorController.MotorID==6)
        {
            //Horizontal Steer
            float constant = 2;

            int newPosition = (int)(m_MotorCurrStartPos-(angleRH-angleLH)*constant);

            m_MotorController.ChangePositionExternal(newPosition);

        }
        else if(m_MotorController.MotorID==2)
        {
            //Vertical Steer
            float constantV = 2f;

            int newPositionV = (int)(m_MotorCurrStartPos+(angleRV-angleLV)*constantV);

            m_MotorController.ChangePositionExternal(newPositionV);
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
