using System.Collections;
using System.Collections.Generic;
//using NUnit.Framework;
using UnityEngine;

public class AvoidanceBehavior : MonoBehaviour
{
    [SerializeField] private float avoidanceDetectionRange;
    [SerializeField] [Range(0f, 1f)] private float avoidanceForce;
    [SerializeField] [Range(-1f, 1f)] private float minDotResult;
    
    private Rigidbody2D concernedRigid;
    
    // Start is called before the first frame update
    void Start()
    {
        concernedRigid = GetComponentInParent<Rigidbody2D>();
    }

    public Vector2 GetAvoidanceVector()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, avoidanceDetectionRange);

        Vector2 steering = Vector2.zero;
        
        foreach (Collider2D col in colliders)
        {
            // Check if it has to avoid the detected collider and if this collider will collide with the other
            if ((col.tag == "Foe" || col.tag == "FoeAttack") && Vector2.Dot((col.transform.position - transform.position).normalized, concernedRigid.velocity.normalized) >= minDotResult)
            {
                steering += Vector2.Perpendicular(col.transform.position - transform.position);
            }
        }

        return steering;
    }
}
