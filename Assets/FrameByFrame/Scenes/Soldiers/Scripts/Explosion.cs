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

    public void Explode()
    {
        Vector3 explosionPos = transform.position + Vector3.up;

        if (FbFManager.IsRecordingOptionEnabled("Explosions"))
        {
            EntityData entity = FbFManager.RecordEntity(this.gameObject);
            EventData deathEvent = entity.AddEvent("Explosion", "Explosions");
            deathEvent.AddProperty("Origin", explosionPos);
            deathEvent.AddProperty("Radius", Radius);
            deathEvent.AddSphere("", explosionPos, Radius, new Color(1.0f, 1.0f, 0.0f, 0.2f), "Explosions");
        }

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
