using FbF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[FrameByFrameRecordingOption("Stats", "Records health, kills, deaths, damage, and death events in the soldiers demo.")]
public class Stats : MonoBehaviour
{
    public int Health = 100;
    public int Kills = 0;
    public int Deads = 0;

    GameObject lastInstigator;

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
        DealDamage(amount, instigator);
    }

    public void ApplyDamage(int amount, GameObject instigator)
    {
        RecordDamage(instigator, amount);
        DealDamage(amount, instigator);
    }

    void DealDamage(int amount, GameObject instigator)
    {
        lastInstigator = instigator;

        // This is introducing a bug for demo purposes
        // We can kill the entity several times
        Health -= amount;
        if (Health <= 0)
        {
            Kill(instigator);
        }
    }

    public bool IsAlive()
    {
        return Health > 0;
    }

    public void Kill(GameObject instigator)
    {
        // Since we can be killed multiple times (incorrectly), kills and deads can be wrong!
        Stats selfStats = instigator.GetComponent<Stats>();
        selfStats.Kills++;

        Deads++;
        RecordDeath();
    }

    public GameObject GetLastInstigator()
    {
        return lastInstigator;
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
            group.AddProperty("Kills", Kills, new Icon("bullseye"));
            group.AddProperty("Deads", Deads, new Icon("skull"));
            group.AddProperty("K/D Ratio", (float)Kills / (float)Deads);
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
