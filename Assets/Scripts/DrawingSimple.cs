using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class DrawingController : MonoBehaviour
{
    [Header("Drawing Settings")]
    [SerializeField] private XRNode drawingControllerNode = XRNode.RightHand; // Controller for drawing
    [SerializeField] private XRNode submitControllerNode = XRNode.LeftHand; // Controller to submit/end drawing
    [SerializeField] private Transform drawPoint; // Tip of controller where drawing starts
    [SerializeField] private Transform playerCamera; // Reference to the VR camera
    [SerializeField] private float canvasDistance = 1.0f; // Distance from player to canvas
    
    [Header("Line Settings")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float minDistance = 0.01f;
    [SerializeField] private Color lineColor = Color.cyan;
    
    [Header("References")]
    [SerializeField] private ShapeRecognizer shapeRecognizer;
    
    private LineRenderer currentLine;
    private List<Vector3> currentPoints = new List<Vector3>();
    private List<GameObject> allLines = new List<GameObject>();
    private bool isDrawing = false;
    private bool newDrawing = true;
    private int lineNumber = 0;
    private GameObject currentLineObject;
    private GameObject canvasPlane; // The invisible drawing plane
    private InputDevice drawingDevice;
    private InputDevice submitDevice;

    void Start()
    {
        // Get the drawing controller (right hand)
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(drawingControllerNode, devices);
        
        if (devices.Count > 0)
        {
            drawingDevice = devices[0];
            Debug.Log("Found drawing device: " + drawingDevice.name + " at node: " + drawingControllerNode);
        }
        else
        {
            Debug.LogWarning("No device found at node: " + drawingControllerNode);
        }
        
        // Get the submit controller (left hand)
        devices.Clear();
        InputDevices.GetDevicesAtXRNode(submitControllerNode, devices);
        
        if (devices.Count > 0)
        {
            submitDevice = devices[0];
            Debug.Log("Found submit device: " + submitDevice.name + " at node: " + submitControllerNode);
        }
        else
        {
            Debug.LogWarning("No device found at node: " + submitControllerNode);
        }
        
        // Auto-find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
            Debug.Log("Auto-assigned main camera as player camera");
        }
        
        // Create the invisible canvas plane
        CreateCanvasPlane();
    }

    void CreateCanvasPlane()
    {
        canvasPlane = new GameObject("DrawingCanvas");
        
        // Position it in front of the player
        UpdateCanvasPosition();
        
        Debug.Log("Canvas plane created at: " + canvasPlane.transform.position);
    }

    void UpdateCanvasPosition()
    {
        if (canvasPlane != null && playerCamera != null)
        {
            // Position canvas in front of player at specified distance
            Vector3 forward = playerCamera.forward;
            forward.y = 0; // Keep canvas vertical (ignore camera pitch)
            forward.Normalize();
            
            canvasPlane.transform.position = playerCamera.position + forward * canvasDistance;
            
            // Rotate canvas to face the player (only Y rotation to keep it vertical)
            canvasPlane.transform.rotation = Quaternion.LookRotation(forward);
        }
    }

    void Update()
    {
        // Update canvas position and rotation to follow player
        UpdateCanvasPosition();
        
        // Reconnect devices if needed
        if (!drawingDevice.isValid)
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(drawingControllerNode, devices);
            if (devices.Count > 0)
            {
                drawingDevice = devices[0];
                Debug.Log("Drawing device reconnected: " + drawingDevice.name);
            }
        }
        
        if (!submitDevice.isValid)
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(submitControllerNode, devices);
            if (devices.Count > 0)
            {
                submitDevice = devices[0];
                Debug.Log("Submit device reconnected: " + submitDevice.name);
            }
        }

        // Check trigger button state (right hand - for drawing)
        bool triggerPressed = false;
        if (drawingDevice.isValid)
        {
            drawingDevice.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);
        }
        
        // Check grip button state (left hand - for submitting)
        bool gripPressed = false;
        if (submitDevice.isValid)
        {
            submitDevice.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed);
        }
        
        // Start drawing when trigger is pressed
        if (triggerPressed && !isDrawing && newDrawing)
        {
            Debug.Log("Starting drawing!");
            newDrawing = false;
            StartDrawing();
        }
        // Continue drawing while trigger is held
        else if (triggerPressed && isDrawing)
        {
            ContinueDrawing();
        }
        // Stop drawing when trigger is released (but keep line visible)
        else if (!triggerPressed && isDrawing)
        {
            Debug.Log("Trigger released - waiting for grip to submit");
            allLines.Add(currentLineObject);
            isDrawing = false;
        }
        // Drawing a new line in the same picture
        else if (triggerPressed && !isDrawing && !newDrawing) 
        {
            NewLine();
        }
        
        // Submit drawing when left grip is pressed (only if we have a drawing)
        if (gripPressed && currentLineObject != null && !isDrawing)
        {
            Debug.Log("Grip pressed - submitting drawing!");
            SubmitDrawing();
        }
    }

    Vector3 ProjectPointOntoCanvas(Vector3 point)
    {
        // Get the canvas plane's position and normal
        Vector3 planeNormal = canvasPlane.transform.forward;
        Vector3 planePoint = canvasPlane.transform.position;
        
        // Project the controller point onto the canvas plane
        Vector3 toPoint = point - planePoint;
        float distance = Vector3.Dot(toPoint, planeNormal);
        Vector3 projectedPoint = point - (planeNormal * distance);
        
        return projectedPoint;
    }

    void StartDrawing()
    {
        
        isDrawing = true;
        currentPoints.Clear();
        
        // Clean up old line if it exists
        if (currentLineObject != null)
        {
            Destroy(currentLineObject);
        }
        
        currentLineObject = new GameObject("DrawnLine");
        currentLine = currentLineObject.AddComponent<LineRenderer>();
        
        
        
        currentLine.material = lineMaterial;
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.startColor = lineColor;
        currentLine.endColor = lineColor;
        currentLine.positionCount = 0;
        currentLine.useWorldSpace = true;
        
        AddPoint(GetDrawPosition());
        
        
    }

    void NewLine() 
    {
        
        isDrawing = true;
        currentPoints.Clear();
        lineNumber++;

        currentLineObject = new GameObject("DrawnLine" + lineNumber);
        currentLine = currentLineObject.AddComponent<LineRenderer>();
        
        
        
        currentLine.material = lineMaterial;
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.startColor = lineColor;
        currentLine.endColor = lineColor;
        currentLine.positionCount = 0;
        currentLine.useWorldSpace = true;
        
        AddPoint(GetDrawPosition());
        
        
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

    void SubmitDrawing()
    {
        // Send points to shape recognizer if we have enough points
        if (currentPoints.Count > 5 && shapeRecognizer != null)
        {
            shapeRecognizer.RecognizeShape(currentPoints);
        }
        else if (currentPoints.Count > 0)
        {
            Debug.Log("Drawing submitted with " + currentPoints.Count + " points (shape recognition not available)");
        }
        
        // Destroy all lines
        if (currentLineObject != null)
        {
            foreach (GameObject line in allLines)
            {
                Destroy(line);
            }
            Destroy(currentLineObject);
            allLines.Clear();
            currentLineObject = null;
        }
        
        currentPoints.Clear();
        newDrawing = true; // Allow new drawing after submission
        lineNumber = 0;
    }

    void AddPoint(Vector3 point)
    {
        // Project the controller position onto the canvas plane
        Vector3 projectedPoint = ProjectPointOntoCanvas(point);
        
        currentPoints.Add(projectedPoint);
        currentLine.positionCount = currentPoints.Count;
        currentLine.SetPosition(currentPoints.Count - 1, projectedPoint);
    }

    Vector3 GetDrawPosition()
    {
        return drawPoint != null ? drawPoint.position : transform.position;
    }

    public void CancelDrawing()
    {
        if (currentLineObject != null)
        {
            foreach (GameObject line in allLines)
            {
                Destroy(line);
            }
            Destroy(currentLineObject);
            allLines.Clear();
            currentLineObject = null;
        }
        currentPoints.Clear();
        isDrawing = false;
        newDrawing = true;
        lineNumber = 0;
    }
}