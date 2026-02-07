using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class DrawingController : MonoBehaviour
{
    [Header("Drawing Settings")]
    [SerializeField] private XRNode controllerNode = XRNode.RightHand; // Choose LeftHand or RightHand
    [SerializeField] private Transform drawPoint; // Tip of controller where drawing starts
    
    [Header("Line Settings")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float minDistance = 0.01f;
    [SerializeField] private Color lineColor = Color.cyan;
    
    //[Header("References")]
    //[SerializeField] private ShapeRecognizer shapeRecognizer;
    
    private LineRenderer currentLine;
    private List<Vector3> currentPoints = new List<Vector3>();
    private bool isDrawing = false;
    private GameObject currentLineObject;
    private InputDevice targetDevice;
    private float fixedZPosition; // Store the Z position of the plane


    void Start()
    {
        // Get the input device for the specified controller
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(controllerNode, devices);
        
        if (devices.Count > 0)
        {
            targetDevice = devices[0];
            Debug.Log("Found device: " + targetDevice.name + " at node: " + controllerNode);
        }
        else
        {
            Debug.LogWarning("No device found at node: " + controllerNode);
        }
    }

    void Update()
    {
        // If device is not valid, try to get it again
        if (!targetDevice.isValid)
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(controllerNode, devices);
            if (devices.Count > 0)
            {
                targetDevice = devices[0];
                Debug.Log("Device reconnected: " + targetDevice.name);
            }
        }

        // Check trigger button state
        bool buttonPressed = false;
        if (targetDevice.isValid)
        {
            targetDevice.TryGetFeatureValue(CommonUsages.triggerButton, out buttonPressed);
        }
        
        Debug.Log("Device Valid: " + targetDevice.isValid + " | Button Pressed: " + buttonPressed + " | Is Drawing: " + isDrawing);

        if (buttonPressed && !isDrawing)
        {
            Debug.Log("CONDITION MET: Starting drawing!");
            StartDrawing();
        }
        else if (buttonPressed && isDrawing)
        {
            ContinueDrawing();
        }
        else if (!buttonPressed && isDrawing)
        {
            Debug.Log("CONDITION MET: Ending drawing!");
            EndDrawing();
        }
    }

    void StartDrawing()
{
    Debug.Log("START DRAWING CALLED!");
    isDrawing = true;
    currentPoints.Clear();
    
    // Capture the initial Z position to define our drawing plane
    fixedZPosition = GetDrawPosition().z;
    Debug.Log("Drawing plane Z position set to: " + fixedZPosition);
    
    currentLineObject = new GameObject("DrawnLine");
    currentLine = currentLineObject.AddComponent<LineRenderer>();
    
    Debug.Log("Line object created: " + currentLineObject.name);
    
    currentLine.material = lineMaterial;
    currentLine.startWidth = lineWidth;
    currentLine.endWidth = lineWidth;
    currentLine.startColor = lineColor;
    currentLine.endColor = lineColor;
    currentLine.positionCount = 0;
    currentLine.useWorldSpace = true;
    
    AddPoint(GetDrawPosition());
    
    Debug.Log("First point added at: " + GetDrawPosition());
}

    void ContinueDrawing()
    {
        Vector3 currentPos = GetDrawPosition();
        
        if (currentPoints.Count == 0 || 
            Vector3.Distance(currentPos, currentPoints[currentPoints.Count - 1]) > minDistance)
        {
            AddPoint(currentPos);
        }
    }

    void EndDrawing()
    {
        isDrawing = false;
        
        //if (currentPoints.Count > 5 && shapeRecognizer != null)
        //{
        //    shapeRecognizer.RecognizeShape(currentPoints);
        //}
        
        if (currentLineObject != null)
        {
            Destroy(currentLineObject, 2f);
        }
    }

    void AddPoint(Vector3 point)
{
    // Force the point to stay on the fixed Z plane
    Vector3 constrainedPoint = new Vector3(point.x, point.y, fixedZPosition);
    
    currentPoints.Add(constrainedPoint);
    currentLine.positionCount = currentPoints.Count;
    currentLine.SetPosition(currentPoints.Count - 1, constrainedPoint);
    
    Debug.Log("Point added! Total points: " + currentPoints.Count + " at position: " + constrainedPoint);
}

    Vector3 GetDrawPosition()
    {
        return drawPoint != null ? drawPoint.position : transform.position;
    }

    public void CancelDrawing()
    {
        if (isDrawing)
        {
            isDrawing = false;
            if (currentLineObject != null)
            {
                Destroy(currentLineObject);
            }
            currentPoints.Clear();
        }
    }
}