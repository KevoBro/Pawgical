using UnityEngine;
using System.Collections.Generic;

public class SpellCastingManager : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField] private UnityEngine.XR.XRNode castingHand = UnityEngine.XR.XRNode.LeftHand;
    [SerializeField] private Transform castPoint; // Left hand position where spells spawn
    
    [Header("Spell Definitions")]
    [SerializeField] private List<SpellData> spells = new List<SpellData>();
    
    [Header("References")]
    [SerializeField] private ShapeRecognizer shapeRecognizer;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private SpellData currentSpell;
    private Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();
    private UnityEngine.XR.InputDevice castingDevice;
    private bool triggerWasPressed = false;
    
    void Start()
    {
        // Get the casting hand controller
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(castingHand, devices);
        
        if (devices.Count > 0)
        {
            castingDevice = devices[0];
            Debug.Log("Found casting device: " + castingDevice.name);
        }
        
        // Subscribe to shape recognition events
        if (shapeRecognizer != null)
        {
            shapeRecognizer.OnShapeRecognized += OnShapeRecognized;
        }
        else
        {
            Debug.LogError("ShapeRecognizer reference not assigned!");
        }
        
        // Initialize cooldown timers for all spells
        foreach (SpellData spell in spells)
        {
            cooldownTimers[spell.shapeName] = 0f;
        }
        
        // Set default spell if available
        if (spells.Count > 0)
        {
            currentSpell = spells[0];
            Debug.Log($"Default spell set to: {currentSpell.shapeName}");
        }
    }
    
    void Update()
    {
        // Reconnect device if needed
        if (!castingDevice.isValid)
        {
            List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(castingHand, devices);
            if (devices.Count > 0)
            {
                castingDevice = devices[0];
            }
        }
        
        // Update all cooldown timers
        UpdateCooldowns();
        
        // Check for trigger press to cast spell
        bool triggerPressed = false;
        if (castingDevice.isValid)
        {
            castingDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed);
        }
        
        // Cast spell on trigger press (not hold)
        if (triggerPressed && !triggerWasPressed)
        {
            TryCastSpell();
        }
        
        triggerWasPressed = triggerPressed;
    }
    
    void OnShapeRecognized(string shapeName, float confidence)
    {
        // Find the spell matching this shape
        SpellData recognizedSpell = spells.Find(s => s.shapeName == shapeName);
        
        if (recognizedSpell != null)
        {
            currentSpell = recognizedSpell;
            if (showDebugInfo)
            {
                Debug.Log($"<color=cyan>Spell switched to: {currentSpell.shapeName}</color>");
            }
        }
        else
        {
            Debug.LogWarning($"No spell found for shape: {shapeName}");
        }
    }
    
    void TryCastSpell()
    {
        Debug.Log("TryCastSpell casted!");
        if (currentSpell == null)
        {
            Debug.LogWarning("No spell selected!");
            return;
        }
        
        // Check if spell is on cooldown
        if (cooldownTimers[currentSpell.shapeName] > 0)
        {
            if (showDebugInfo)
            {
                Debug.Log($"Spell {currentSpell.shapeName} on cooldown: {cooldownTimers[currentSpell.shapeName]:F1}s remaining");
            }
            return;
        }
        
        // Cast the spell
        CastSpell(currentSpell);
        
        // Start cooldown
        cooldownTimers[currentSpell.shapeName] = currentSpell.cooldown;
    }
    
    void CastSpell(SpellData spell)
{
    if (spell.projectilePrefab == null)
    {
        Debug.LogError($"No projectile prefab assigned for spell: {spell.shapeName}");
        return;
    }
    
    // Spawn projectile at cast point
    Vector3 spawnPosition = castPoint != null ? castPoint.position : transform.position;
    Quaternion spawnRotation = castPoint != null ? castPoint.rotation : transform.rotation;
    
    GameObject projectile = Instantiate(spell.projectilePrefab, spawnPosition, spawnRotation);
    
    // Get any component that implements IProjectile
    IProjectile projectileScript = projectile.GetComponent<IProjectile>();
    
    if (projectileScript != null)
    {
        projectileScript.Initialize(spell.damage, spell.projectileSpeed, spell.lifetime);
    }
    else
    {
        Debug.LogError($"Projectile prefab for {spell.shapeName} doesn't have a script implementing IProjectile!");
    }
    
    if (showDebugInfo)
    {
        Debug.Log($"<color=green>Cast {spell.shapeName}! Cooldown: {spell.cooldown}s</color>");
    }
}
    
    void UpdateCooldowns()
    {
        // Update all cooldown timers
        List<string> keys = new List<string>(cooldownTimers.Keys);
        foreach (string key in keys)
        {
            if (cooldownTimers[key] > 0)
            {
                cooldownTimers[key] -= Time.deltaTime;
                if (cooldownTimers[key] < 0)
                {
                    cooldownTimers[key] = 0;
                }
            }
        }
    }
    
    // Public methods for UI or other systems
    public float GetCooldownPercentage(string shapeName)
    {
        SpellData spell = spells.Find(s => s.shapeName == shapeName);
        if (spell != null && cooldownTimers.ContainsKey(shapeName))
        {
            return 1f - (cooldownTimers[shapeName] / spell.cooldown);
        }
        return 1f; // Ready
    }
    
    public string GetCurrentSpellName()
    {
        return currentSpell != null ? currentSpell.shapeName : "None";
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (shapeRecognizer != null)
        {
            shapeRecognizer.OnShapeRecognized -= OnShapeRecognized;
        }
    }
}

[System.Serializable]
public class SpellData
{
    public string shapeName; // Must match template name in ShapeRecognizer
    public GameObject projectilePrefab;
    public float damage = 10f;
    public float projectileSpeed = 10f;
    public float cooldown = 1f;
    public float lifetime = 5f; // How long projectile exists before destroying
}