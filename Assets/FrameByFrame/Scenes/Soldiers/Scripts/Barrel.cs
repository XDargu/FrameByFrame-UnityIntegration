using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FbF;

public class Barrel : MonoBehaviour
{
    public GameObject explosion;
    public float Radius = 5.0f;

    Stats statsComponent;

    // Start is called before the first frame update
    void Start()
    {
        statsComponent = GetComponent<Stats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!statsComponent.IsAlive())
        {
            Explode();
        }
    }

    void Explode()
    {
        GetComponent<Explosion>().Explode();
        Destroy(gameObject);
    }
}
