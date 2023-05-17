using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class WheelSelector : MonoBehaviour
{
    [SerializeField] private InputActionReference m_WheelActivate;

    [SerializeField] private GameObject m_SelectionVisualPrefab;

    [SerializeField] private GameObject m_LeftController;

    [SerializeField] private GameObject m_WheelOrientTo;

    [SerializeField] 
    private GameObject m_XROrigin;
    
    private Vector3 m_WheelCenter;

    private Vector3 m_CameraRightDirection;

    private float m_Radius = 0.25f;

    private int m_NumofOptions = 4;

    private GameObject[] m_WheelChoices;

    private int m_DegSections;

    private int m_WheelSelectionTemp;

    private int m_FinalWheelSelection;

    private string[] m_FilterNames = {"1","4", "3", "2", "Motor 5", "Motor 6"};
    
    public static event Action<int,Color> onMotorWheelSelected;
    
    public int WheelSelection
    {
        get => m_FinalWheelSelection;
        set => m_FinalWheelSelection = value;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        m_WheelChoices = new GameObject[m_NumofOptions];

        m_WheelActivate.action.Enable();
        SetupCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void SetupCallbacks()
    {
        m_WheelActivate.action.started += ctx =>
        {
            m_WheelCenter = m_LeftController.transform.position;

            m_CameraRightDirection = m_WheelOrientTo.transform.right;
            
            Debug.Log($"Forward: {m_LeftController.transform.forward}");
            
            m_DegSections = 360 / m_NumofOptions;
            
            for (int i = 0; i < m_NumofOptions; i++)
            {
                Vector2 newposition = PolarToCartesian(m_Radius / 2, ((m_DegSections*i)+(m_DegSections)));
                
                Debug.Log($"newPosition: {newposition}");
                Debug.Log($"m_WheelCenter: {m_WheelCenter}");
                
                //Change this
                Transform newTransform = m_SelectionVisualPrefab.transform;
                
                newTransform.rotation = Quaternion.LookRotation(m_WheelOrientTo.transform.right,m_WheelOrientTo.transform.forward);
                
                newTransform.position = new Vector3((m_WheelCenter.x + m_CameraRightDirection.x*newposition.x), m_WheelCenter.y + newposition.y, (m_WheelCenter.z + m_CameraRightDirection.z*newposition.x));

                m_WheelChoices[i] = Instantiate(m_SelectionVisualPrefab,newTransform);
                
                //m_WheelChoices[i].GetComponent<MeshRenderer>().material.color = Color.HSVToRGB((m_DegSections*i)/360f,.5f,1);
                m_WheelChoices[i].GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(120f/360f,.5f,(((m_DegSections*i)/360f)*0.6f)+0.4f);
                
                m_WheelChoices[i].GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
                m_WheelChoices[i].GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.HSVToRGB(120f/360f,.5f,(((m_DegSections*i)/360f)*0.6f)+0.4f));

                m_WheelChoices[i].GetComponentInChildren<TextMeshProUGUI>().text = m_FilterNames[i];
                
            }
            
        };
        m_WheelActivate.action.performed += ctx =>
        {
            Vector3 pointerPosition = m_LeftController.transform.position;
            
            //Selection Wheel Math Here
            // Vector2 Pointer_WheelRelative = CartesianToPolar(m_PointerPosition.x-m_WheelCenter.x, m_PointerPosition.y-m_WheelCenter.y);
            // m_WheelSelection_temp = Mathf.FloorToInt(Pointer_WheelRelative.y / m_DegSections);
            
            // Vector3 Pointer_Spherical = CartesianToSpherical(new Vector3(m_PointerPosition.x-m_WheelCenter.x,m_PointerPosition.z-m_WheelCenter.z,m_PointerPosition.y-m_WheelCenter.y));
            //m_WheelSelection_temp = Mathf.FloorToInt((float)(Pointer_Spherical.y*(180/Math.PI)) / m_DegSections);

            Vector3 vecA = pointerPosition - m_WheelCenter;
            Vector3 vecB = m_CameraRightDirection;
            
            float angle = Mathf.Acos((vecA.x*vecB.x+vecA.y*vecB.y+vecA.z*vecB.z)/(Mathf.Sqrt(vecA.x*vecA.x+vecA.y*vecA.y+vecA.z*vecA.z)*Mathf.Sqrt(vecB.x*vecB.x+vecB.y*vecB.y+vecB.z*vecB.z)))*Mathf.Rad2Deg;
            
            if (vecA.y < 0)
            {
                angle = 360 - angle - (m_DegSections / 2);
            }
            else if (angle < m_DegSections/2)
            {
                angle = 360 + (angle - m_DegSections / 2);
            }
            else
            {
                angle -= (m_DegSections / 2);
            }

            if (pointerPosition.x - m_WheelCenter.x > m_Radius || pointerPosition.y - m_WheelCenter.y > m_Radius ||
                pointerPosition.z - m_WheelCenter.z > m_Radius)
            {
                m_WheelSelectionTemp = m_WheelSelectionTemp;
            }
            else if (pointerPosition - m_WheelCenter == Vector3.zero)
            {
                if (m_WheelSelectionTemp != 0)
                {
                    m_WheelChoices[m_WheelSelectionTemp].transform.localScale = new Vector3(0.075f,0.075f,0.075f);
                    m_WheelChoices[m_WheelSelectionTemp].GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(120f/360f,.5f,(((m_DegSections*m_WheelSelectionTemp)/360f)*0.6f)+0.4f);
                    m_WheelChoices[m_WheelSelectionTemp].GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.HSVToRGB(120f/360f,.5f,(((m_DegSections*m_WheelSelectionTemp)/360f)*0.6f)+0.4f));
                    
                    m_LeftController.GetComponent<XRBaseController>().SendHapticImpulse(1,.1f);
                }
                m_WheelSelectionTemp = 0;
            }
            else
            {
                if (m_WheelSelectionTemp != Mathf.FloorToInt((angle) / m_DegSections))
                {
                    m_WheelChoices[m_WheelSelectionTemp].transform.localScale = new Vector3(0.075f,0.075f,0.075f);
                    m_WheelChoices[m_WheelSelectionTemp].GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(120f/360f,.5f,(((m_DegSections*m_WheelSelectionTemp)/360f)*0.6f)+0.4f);
                    m_WheelChoices[m_WheelSelectionTemp].GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.HSVToRGB(120f/360f,.5f,(((m_DegSections*m_WheelSelectionTemp)/360f)*0.6f)+0.4f));
                    
                    m_LeftController.GetComponent<XRBaseController>().SendHapticImpulse(1,.1f);
                }
                //m_WheelSelection_temp = Mathf.FloorToInt((float)(angle) / m_DegSections);
                m_WheelSelectionTemp = (int)((angle) / m_DegSections);
            }

            if (m_WheelSelectionTemp < 0) m_WheelSelectionTemp = 0;
            
            Debug.Log($"Angle: {angle}");
            Debug.Log($"m_DegSections: {m_DegSections}");
            Debug.Log($"Wheel Selection Number: {m_WheelSelectionTemp}");
            
            m_WheelChoices[m_WheelSelectionTemp].GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(120f/360f,1,(((m_DegSections*m_WheelSelectionTemp)/360f)*0.6f)+0.4f);
            m_WheelChoices[m_WheelSelectionTemp].GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.HSVToRGB(120f/360f,1,(((m_DegSections*m_WheelSelectionTemp)/360f)*0.6f)+0.4f));
            m_WheelChoices[m_WheelSelectionTemp].transform.localScale = new Vector3(0.085f,0.085f,0.085f);

        };
        m_WheelActivate.action.canceled += _ =>
        {
            Color selectedColor = m_WheelChoices[m_WheelSelectionTemp].GetComponent<MeshRenderer>().material.color;
            
            for (int i = 0; i < m_NumofOptions; i++)
            {
                Destroy(m_WheelChoices[i]);
            }
            m_WheelChoices = new GameObject[m_NumofOptions];

            m_FinalWheelSelection = m_WheelSelectionTemp;
            onMotorWheelSelected?.Invoke(m_FinalWheelSelection,selectedColor);

        };
    }

    private Vector2 CartesianToPolar(float x, float y)
    {
        float radius = Mathf.Sqrt((x * x) + (y * y));
        float theta = Mathf.Atan2(y,x);
        
        
        
        return new Vector2(radius,  (theta*Mathf.Rad2Deg));
    }

    private Vector2 PolarToCartesian(float r, float theta)
    {
        float x = (r * Mathf.Cos(theta*Mathf.Deg2Rad));
        float y = (r * Mathf.Sin(theta*Mathf.Deg2Rad));

        return new Vector2(x, y);


    }
    
    public static Vector3 CartesianToSpherical(Vector3 cartCoords)
    {
        if (cartCoords.x == 0) cartCoords.x = Mathf.Epsilon;
        float outRadius = Mathf.Sqrt((cartCoords.x * cartCoords.x)
                                     + (cartCoords.y * cartCoords.y)
                                     + (cartCoords.z * cartCoords.z));
        float outPhi = Mathf.Atan(cartCoords.z / cartCoords.x);
        if (cartCoords.x < 0) outPhi += Mathf.PI;
        float outTheta = Mathf.Asin(cartCoords.y / outRadius);
    
        return new Vector3(outRadius, outPhi, outTheta);
    }
}
