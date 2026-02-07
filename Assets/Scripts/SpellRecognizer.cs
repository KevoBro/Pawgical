using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ShapeRecognizer : MonoBehaviour
{
    [Header("Recognition Settings")]
    [SerializeField] private int resamplePoints = 64;
    [SerializeField] private float recognitionThreshold = 0.7f;
    
    [Header("Shape Templates")]
    [SerializeField] private List<ShapeTemplate> templates = new List<ShapeTemplate>();
    
    [Header("Template Recording")]
    [SerializeField] private bool recordingMode = false; // Toggle this to enter recording mode
    [SerializeField] private string templateName = "NewShape"; // Name for the next template
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    public delegate void ShapeRecognizedHandler(string shapeName, float confidence);
    public event ShapeRecognizedHandler OnShapeRecognized;

    void Start()
    {
        // Load previously saved templates from file
        LoadTemplatesFromFile();
        if (templates.Count == 0)
        {
            CreateDefaultTemplates();
        }

        Debug.Log($"ShapeRecognizer started with {templates.Count} templates");
    }

    public void RecognizeShape(List<Vector3> points)
    {
        if (points == null || points.Count < 5)
        {
            Debug.LogWarning("Not enough points to recognize shape");
            return;
        }

        // If in recording mode, save as template instead of recognizing
        if (recordingMode)
        {
            SaveAsTemplate(points, templateName);
            return;
        }

        Debug.Log($"Recognizing shape from {points.Count} points");

        List<Vector3> normalizedPoints = NormalizeShape(points);

        float bestScore = float.MaxValue;
        string bestMatch = "Unknown";
        
        foreach (ShapeTemplate template in templates)
        {
            float score = CompareShapes(normalizedPoints, template.points);
            
            if (showDebugInfo)
            {
                Debug.Log($"Template '{template.shapeName}' score: {score}");
            }
            
            if (score < bestScore)
            {
                bestScore = score;
                bestMatch = template.shapeName;
            }
        }

        float confidence = 1f - (bestScore / 4f);
        confidence = Mathf.Clamp01(confidence);

        if (showDebugInfo)
        {
            Debug.Log($"Best match: {bestMatch} with confidence: {confidence:F2}");
        }

        if (confidence >= recognitionThreshold)
        {
            Debug.Log($"<color=green>Shape recognized: {bestMatch} ({confidence:P0})</color>");
            OnShapeRecognized?.Invoke(bestMatch, confidence);
        }
        else
        {
            Debug.Log($"<color=yellow>No confident match (best was {bestMatch} at {confidence:P0})</color>");
        }
    }

    List<Vector3> NormalizeShape(List<Vector3> points)
    {
        List<Vector3> resampled = Resample(points, resamplePoints);
        resampled = RotateToZero(resampled);
        resampled = ScaleToSquare(resampled, 1f);
        resampled = TranslateToOrigin(resampled);
        return resampled;
    }

    List<Vector3> Resample(List<Vector3> points, int n)
    {
        float totalLength = PathLength(points);
        float interval = totalLength / (n - 1);
        float distanceSoFar = 0;
        
        List<Vector3> newPoints = new List<Vector3> { points[0] };
        
        for (int i = 1; i < points.Count; i++)
        {
            float distance = Vector3.Distance(points[i - 1], points[i]);
            
            if (distanceSoFar + distance >= interval)
            {
                float t = (interval - distanceSoFar) / distance;
                Vector3 point = Vector3.Lerp(points[i - 1], points[i], t);
                newPoints.Add(point);
                points.Insert(i, point);
                distanceSoFar = 0;
            }
            else
            {
                distanceSoFar += distance;
            }
        }
        
        if (newPoints.Count < n)
        {
            newPoints.Add(points[points.Count - 1]);
        }
        
        return newPoints;
    }

    float PathLength(List<Vector3> points)
    {
        float length = 0;
        for (int i = 1; i < points.Count; i++)
        {
            length += Vector3.Distance(points[i - 1], points[i]);
        }
        return length;
    }

    List<Vector3> RotateToZero(List<Vector3> points)
    {
        Vector3 centroid = Centroid(points);
        float angle = Mathf.Atan2(
            centroid.y - points[0].y,
            centroid.x - points[0].x
        );
        return RotateBy(points, -angle);
    }

    List<Vector3> RotateBy(List<Vector3> points, float angle)
    {
        Vector3 centroid = Centroid(points);
        List<Vector3> rotated = new List<Vector3>();
        
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        
        foreach (Vector3 p in points)
        {
            float qx = (p.x - centroid.x) * cos - (p.y - centroid.y) * sin + centroid.x;
            float qy = (p.x - centroid.x) * sin + (p.y - centroid.y) * cos + centroid.y;
            rotated.Add(new Vector3(qx, qy, p.z));
        }
        
        return rotated;
    }

    List<Vector3> ScaleToSquare(List<Vector3> points, float size)
    {
        Bounds bounds = GetBounds(points);
        List<Vector3> scaled = new List<Vector3>();
        
        float scaleX = size / bounds.size.x;
        float scaleY = size / bounds.size.y;
        
        foreach (Vector3 p in points)
        {
            scaled.Add(new Vector3(
                p.x * scaleX,
                p.y * scaleY,
                p.z
            ));
        }
        
        return scaled;
    }

    List<Vector3> TranslateToOrigin(List<Vector3> points)
    {
        Vector3 centroid = Centroid(points);
        List<Vector3> translated = new List<Vector3>();
        
        foreach (Vector3 p in points)
        {
            translated.Add(p - centroid);
        }
        
        return translated;
    }

    Vector3 Centroid(List<Vector3> points)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 p in points)
        {
            sum += p;
        }
        return sum / points.Count;
    }

    Bounds GetBounds(List<Vector3> points)
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        
        foreach (Vector3 p in points)
        {
            minX = Mathf.Min(minX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxX = Mathf.Max(maxX, p.x);
            maxY = Mathf.Max(maxY, p.y);
        }
        
        return new Bounds(
            new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0),
            new Vector3(maxX - minX, maxY - minY, 0)
        );
    }

    float CompareShapes(List<Vector3> points1, List<Vector3> points2)
    {
        float distance = 0;
        
        for (int i = 0; i < Mathf.Min(points1.Count, points2.Count); i++)
        {
            distance += Vector3.Distance(points1[i], points2[i]);
        }
        
        return distance / points1.Count;
    }

    void CreateDefaultTemplates()
    {
        Debug.Log("Creating default shape templates");
    }

    public void SaveAsTemplate(List<Vector3> points, string shapeName)
    {
        List<Vector3> normalized = NormalizeShape(points);
        ShapeTemplate template = new ShapeTemplate
        {
            shapeName = shapeName,
            points = normalized
        };
        templates.Add(template);
        
        Debug.Log($"<color=cyan>★ TEMPLATE SAVED: {shapeName} with {normalized.Count} points ★</color>");
        
        // Save to file so it persists
        SaveTemplatesToFile();
    }

    void SaveTemplatesToFile()
    {
        string path = Application.persistentDataPath + "/spell_templates.json";
        TemplateCollection collection = new TemplateCollection { templates = templates };
        string json = JsonUtility.ToJson(collection, true);
        File.WriteAllText(path, json);
        Debug.Log($"Templates saved to: {path}");
    }

    public void LoadTemplatesFromFile()
    {
        string path = Application.persistentDataPath + "/spell_templates.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            TemplateCollection collection = JsonUtility.FromJson<TemplateCollection>(json);
            templates = collection.templates;
            Debug.Log($"Loaded {templates.Count} templates from file");
        }
        else
        {
            Debug.Log("No template file found");
        }
    }

    [ContextMenu("Load Templates")]
    void LoadTemplatesMenu()
    {
        LoadTemplatesFromFile();
    }

    [ContextMenu("Clear All Templates")]
    void ClearTemplates()
    {
        templates.Clear();
        Debug.Log("All templates cleared");
    }

    [ContextMenu("List Templates")]
    void ListTemplates()
    {
        Debug.Log($"=== {templates.Count} Templates ===");
        foreach (var t in templates)
        {
            Debug.Log($"- {t.shapeName} ({t.points.Count} points)");
        }
    }
}

[System.Serializable]
public class ShapeTemplate
{
    public string shapeName;
    public List<Vector3> points;
}

[System.Serializable]
public class TemplateCollection
{
    public List<ShapeTemplate> templates;
}
/*

## Step 2: How to Record Templates

### Recording Workflow:

1. **Enter Recording Mode**:
   - Select your ShapeRecognizer GameObject
   - Check the **Recording Mode** checkbox
   - Set **Template Name** to what you want (e.g., "Circle", "Triangle", "Fireball")

2. **Draw the Shape in VR**:
   - Put on your headset
   - Draw the shape with your right controller
   - Submit it with left grip
   - Check the Console - you should see "★ TEMPLATE SAVED: [name] ★"

3. **Repeat for Each Spell**:
   - Change **Template Name** to next shape (e.g., "Lightning")
   - Draw the new shape
   - Submit
   - Repeat

4. **Exit Recording Mode**:
   - Uncheck **Recording Mode**
   - Now when you draw, it will recognize shapes instead of recording

### Recommended Spells to Start With:

**Easy to recognize:**
- **Circle** - Draw a circle
- **Triangle** - Draw a triangle
- **Line** - Draw a straight line
- **Zigzag** - Draw a lightning bolt shape
- **Spiral** - Draw a spiral

**Tips for good templates:**
- Draw each shape 2-3 times and save with names like "Circle1", "Circle2", "Circle3"
- More templates = better recognition
- Be consistent in how you draw (clockwise vs counterclockwise)
- Draw at a comfortable speed

## Step 3: Testing Recognition

After recording templates:

1. **Uncheck Recording Mode**
2. **Draw a shape in VR**
3. **Check Console** - should say something like:
```
   Shape recognized: Circle (85%)
```

## Step 4: Persistence

Your templates are automatically saved to a JSON file at:
```
Application.persistentDataPath + "/spell_templates.json"

*/