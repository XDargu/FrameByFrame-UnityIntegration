using FbF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class Stats : MonoBehaviour
{
    public int Health = 100;

    // Start is called before the first frame update
    void Start()
    {
        FbFManager.RegisterRecordingOption("Stats");
    }

    // Update is called once per frame
    void Update()
    {
        if (FbFManager.IsRecordingOptionEnabled("Stats"))
        {
            EntityData entity = FbFManager.RecordEntity(this.gameObject);
            PropertyGroup group = entity.AddPropertyGroup("Stats");
            group.AddProperty("Health", Health);
        }
    }

    public void ApplyDamage(DamageFallOff falloff, Vector3 origin, Vector3 destination, GameObject instigator)
    {
        float distance = (origin - destination).magnitude;
        int amount = falloff.CalculateDamage(distance);

        if (FbFManager.IsRecordingOptionEnabled("Stats"))
        {
            EntityData entity = FbFManager.RecordEntity(this.gameObject);
            EventData damageEvent = entity.AddEvent("Damage received", "Damage");
            falloff.RecordProperties(damageEvent);
            damageEvent.AddProperty("Final amount", amount);
            damageEvent.AddEntityRef("Instigator", instigator);
            damageEvent.AddLine("", origin, destination, Color.red, "Stats");
        }

        DealDamage(amount);
    }

    public void ApplyDamage(int amount, GameObject instigator)
    {
        if (FbFManager.IsRecordingOptionEnabled("Stats"))
        {
            EntityData entity = FbFManager.RecordEntity(this.gameObject);
            EventData damageEvent = entity.AddEvent("Damage received", "Damage");
            damageEvent.AddProperty("Amount", amount);
            damageEvent.AddEntityRef("Instigator", instigator);
        }

        DealDamage(amount);
    }

    void DealDamage(int amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Kill();
        }
    }

    public bool IsAlive()
    {
        return Health > 0;
    }

    public void Kill()
    {
        if (FbFManager.IsRecordingOptionEnabled("Stats"))
        {
            EntityData entity = FbFManager.RecordEntity(this.gameObject);
            EventData deathEvent = entity.AddEvent("Death", "Damage");
            deathEvent.AddProperty("Health", Health);
        }
    }
}