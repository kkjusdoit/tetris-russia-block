using UnityEngine;

public class BottomBorder : MonoBehaviour
{
    public BoxCollider borderCollider;
    private Rigidbody rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        borderCollider = GetComponent<BoxCollider>();
        if (borderCollider == null)
        {
            Debug.LogError("BottomBorder: BoxCollider component not found!");
        }
        borderCollider.isTrigger = true;
        
        // Add Rigidbody for trigger detection
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true; // Prevent physics from affecting position
        rb.useGravity = false; // Don't let gravity affect it
    }


}