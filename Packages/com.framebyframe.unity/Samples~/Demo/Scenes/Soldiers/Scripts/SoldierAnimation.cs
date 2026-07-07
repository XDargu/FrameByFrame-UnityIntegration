using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoldierAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    Animator animator;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Speed", GetComponent<NavMeshAgent>().velocity.magnitude);
    }

    void OnCollisionEnter(Collision col)
    {
        animator.SetTrigger("Shoot");
    }
}
