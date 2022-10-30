using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using FbF;

public class Weapon : MonoBehaviour
{
    public ParticleSystem projectileEffect;
    public DamageFallOff falloff;

    // Start is called before the first frame update
    void Start()
    {
        FbFManager.RegisterRecordingOption("Weapons");
    }

    public void Shoot(GameObject target, Vector3 origin, Vector3 destination)
    {
        target.GetComponent<Stats>().ApplyDamage(falloff, origin, destination, gameObject);

        ParticleSystem projectile = GameObject.Instantiate(projectileEffect);
        projectile.transform.position = origin;
        projectile.transform.LookAt(destination, Vector3.up);
        ParticleSystem.MainModule main = projectile.main;
        float distanceToTarget = (origin - destination).magnitude;
        main.startLifetime = distanceToTarget / main.startSpeed.constant;
    }
}
