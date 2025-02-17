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
        RecordStats();
    }

    public void ApplyDamage(DamageFallOff falloff, Vector3 origin, Vector3 destination, GameObject instigator)
    {
        float distance = (origin - destination).magnitude;
        int amount = falloff.CalculateDamage(distance);

        RecordDamage(falloff, origin, destination, instigator, amount);
        DealDamage(amount);
    }

    public void ApplyDamage(int amount, GameObject instigator)
    {
        RecordDamage(instigator, amount);
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
        RecordDeath();
    }

    [System.Diagnostics.Conditional("DEBUG")]
    void RecordDamage(DamageFallOff falloff, Vector3 origin, Vector3 destination, GameObject instigator, int amount)
    {
        if (FbFManager.IsRecordingOptionEnabled("Stats"))
        {
            PropertyGroup damageEvent = FbFManager.RecordEvent(this.gameObject, "Damage received", "Damage");
            falloff.RecordProperties(damageEvent);
            damageEvent.AddProperty("Final amount", amount);
            damageEvent.AddEntityRef("Instigator", instigator);
            damageEvent.AddLine("", origin, destination, Color.red, "Stats");
        }
    }

    [System.Diagnostics.Conditional("DEBUG")]
    void RecordDamage(GameObject instigator, int amount)
    {
        if (FbFManager.IsRecordingOptionEnabled("Stats"))
        {
            PropertyGroup damageEvent = FbFManager.RecordEvent(this.gameObject, "Damage received", "Damage");
            damageEvent.AddProperty("Final amount", amount);
            damageEvent.AddEntityRef("Instigator", instigator);
        }
    }

    [System.Diagnostics.Conditional("DEBUG")]
    void RecordStats()
    {
        if (FbFManager.IsRecordingOptionEnabled("Stats"))
        {
            EntityData entity = FbFManager.RecordEntity(this.gameObject);
            PropertyGroup group = entity.AddGroup("Stats");
            group.AddProperty("Health", Health, new Icon("heart"));
        }
    }

    [System.Diagnostics.Conditional("DEBUG")]
    void RecordDeath()
    {
        if (FbFManager.IsRecordingOptionEnabled("Stats"))
        {
            PropertyGroup deathEvent = FbFManager.RecordEvent(this.gameObject, "Death", "Damage");
            deathEvent.AddProperty("Health", Health);
        }
    }
}