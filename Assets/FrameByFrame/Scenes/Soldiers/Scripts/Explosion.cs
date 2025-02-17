using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FbF;
using Utils;

public class Explosion : MonoBehaviour
{
    public ParticleSystem explosionParticles;
    public float Radius = 5.0f;
    public float Force = 2000.0f;
    public DamageFallOff falloff;

    // Start is called before the first frame update
    void Start()
    {
        FbFManager.RegisterRecordingOption("Explosions");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [System.Diagnostics.Conditional("DEBUG")]
    void RecordExplosion(Vector3 explosionPos)
    {
        if (FbFManager.IsRecordingOptionEnabled("Explosions"))
        {
            PropertyGroup explosionEvent = FbFManager.RecordEvent(this.gameObject, "Explosion", "Explosions");
            explosionEvent.AddProperty("Origin", explosionPos);
            explosionEvent.AddProperty("Radius", Radius);
            explosionEvent.AddSphere("Explosion", explosionPos, Radius, new Color(1.0f, 1.0f, 0.0f, 0.2f), "Explosions", null, PropertyFlags.Hidden);
        }
    }

    public void Explode()
    {
        Vector3 explosionPos = transform.position + Vector3.up;

        RecordExplosion(explosionPos);

        ParticleSystem explosionObject = GameObject.Instantiate(explosionParticles);
        explosionObject.transform.position = explosionPos;

        Stats[] statObjects = GameObject.FindObjectsOfType<Stats>();

        foreach (Stats statObj in statObjects)
        {
            float distanceToObject = Vector3.Distance(statObj.transform.position, transform.position);
            if (distanceToObject < Radius)
            {
                statObj.ApplyDamage(falloff.CalculateDamage(distanceToObject), gameObject);
            }
        }

        // Physics explosion
        Collider[] colliders = Physics.OverlapSphere(explosionPos, Radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(Force, explosionPos, Radius, 3.0F);
        }
    }
}
