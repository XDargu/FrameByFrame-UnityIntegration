using FbF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum Faction
{
    Blue,
    Red
}

class StateMachine
{
    State currentState;
    public SoldierAI soldier;
    public StateMachine(State state, SoldierAI soldier)
    {
        this.currentState = state;
        this.soldier = soldier;

        currentState.OnStart(this);
    }

    public void TransitionToState(State state)
    {
        currentState.OnEnd(this);
        currentState = state;
        currentState.OnStart(this);
    }

    public void Update()
    {
        currentState.Update(this);
        RecordUpdate();
    }

    [System.Diagnostics.Conditional("DEBUG")]
    void RecordUpdate()
    {
        if (FbFManager.IsRecordingOptionEnabled("AI"))
        {
            EntityData entity = FbFManager.RecordEntity(soldier.gameObject);
            PropertyGroup group = entity.AddPropertyGroup("State Machine");
            group.AddProperty("Current State", currentState.ToString());

            currentState.UpdateDebug(this, entity);
        }
    }
}

class State
{
    public virtual void Update(StateMachine fsm) { }
    public virtual void UpdateDebug(StateMachine fsm, EntityData entity) { }
    public virtual void OnStart(StateMachine fsm) { }
    public virtual void OnEnd(StateMachine fsm) { }

    public StateMachine fsm;
}

class Wandering : State
{
    float timeUntilChange = 0;

    public override void Update(StateMachine fsm)
    {
        UpdateMovement(fsm);
        DetectEnemies(fsm);
    }

    void UpdateMovement(StateMachine fsm)
    {
        timeUntilChange -= Time.deltaTime;
        if (timeUntilChange <= 0)
        {
            Vector3 randomPos = fsm.soldier.transform.position;
            if (TryRandomNavmeshLocation(10.0f, fsm.soldier.initialPos, out randomPos))
            {
                fsm.soldier.agent.SetDestination(randomPos);
            }
            timeUntilChange = Random.Range(2.0f, 5.0f);
        }
    }

    void DetectEnemies(StateMachine fsm)
    {
        SoldierAI[] soldiers = GameObject.FindObjectsOfType<SoldierAI>();

        foreach(SoldierAI soldier in soldiers)
        {
            if (soldier.faction != fsm.soldier.faction)
            {
                if (Vector3.Distance(soldier.transform.position, fsm.soldier.transform.position) < fsm.soldier.DetectionRange)
                {
                    fsm.TransitionToState(new Chasing(soldier));
                }
            }
        }
    }

    bool TryRandomNavmeshLocation(float radius, Vector3 centre, out Vector3 position)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius + centre;

        NavMeshHit hit;
        bool success = NavMesh.SamplePosition(randomDirection, out hit, radius, 1);
        position = hit.position;

        return success;
    }
}

class Chasing : State
{
    SoldierAI target;

    public Chasing(SoldierAI target)
    {
        this.target = target;
    }

    public override void Update(StateMachine fsm)
    {
        if (target == null)
        {
            fsm.TransitionToState(new Wandering());
            return;
        }

        if (!target.GetComponent<Stats>().IsAlive())
        {
            fsm.TransitionToState(new Wandering());
        }

        if (Vector3.Distance(target.transform.position, fsm.soldier.transform.position) < fsm.soldier.ShootingRange)
        {
            fsm.TransitionToState(new Shooting(target));
        }

        if (Vector3.Distance(target.transform.position, fsm.soldier.transform.position) > fsm.soldier.DetectionRange + 0.5f)
        {
            fsm.TransitionToState(new Wandering());
        }

        fsm.soldier.agent.SetDestination(target.transform.position);
    }

    public override void UpdateDebug(StateMachine fsm, EntityData entity)
    {
        PropertyGroup group = entity.AddPropertyGroup("State Machine");
        group.AddLine("", fsm.soldier.transform.position + Vector3.up, target.transform.position + Vector3.up, Color.yellow, "AI");
        group.AddEntityRef("Current Target", target.gameObject);
    }

}

class Shooting : State
{
    SoldierAI target;
    float timeUntilShoot = Random.Range(0.8f, 1.2f);

    public Shooting(SoldierAI target)
    {
        this.target = target;
    }

    public override void Update(StateMachine fsm)
    {
        if (target == null)
        {
            fsm.TransitionToState(new Wandering());
            return;
        }

        if (!target.GetComponent<Stats>().IsAlive())
        {
            fsm.TransitionToState(new Wandering());
        }

        if (Vector3.Distance(target.transform.position, fsm.soldier.transform.position) > fsm.soldier.ShootingRange + 0.5f)
        {
            fsm.TransitionToState(new Chasing(target));
        }

        // Shoot every X seconds
        timeUntilShoot -= Time.deltaTime;
        if (timeUntilShoot < 0)
        {
            Barrel nearbyBarrel = FindBarrelNearbyPos(target.transform.position, 3.0f);
            GameObject shootingTarget = nearbyBarrel ? nearbyBarrel.gameObject : target.gameObject;

            fsm.soldier.GetComponent<Weapon>().Shoot(shootingTarget, shootingTarget.transform.position + Vector3.up * 1.8f, fsm.soldier.transform.position + Vector3.up * 1.8f);
            fsm.soldier.GetComponent<Animator>().SetTrigger("Shoot");

            timeUntilShoot = Random.Range(0.8f, 1.5f);
        }
        
        FaceTarget(target.transform.position, fsm.soldier.transform);
    }

    public override void UpdateDebug(StateMachine fsm, EntityData entity)
    {
        PropertyGroup group = entity.AddPropertyGroup("State Machine");
        group.AddLine("", fsm.soldier.transform.position + Vector3.up, target.transform.position + Vector3.up, Color.red, "AI");
        group.AddEntityRef("Current Target", target.gameObject, new Icon("dot-circle"));
        group.AddProperty("Time until shooting", timeUntilShoot, new Icon("clock"));
    }

    public override void OnStart(StateMachine fsm) { fsm.soldier.agent.isStopped = true; }
    public override void OnEnd(StateMachine fsm) { fsm.soldier.agent.isStopped = false; }

    private void FaceTarget(Vector3 destination, Transform transform)
    {
        Vector3 lookPos = destination - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.2f);
    }

    private Barrel FindBarrelNearbyPos(Vector3 position, float range)
    {
        Barrel[] barrels = GameObject.FindObjectsOfType<Barrel>();

        foreach (Barrel barrel in barrels)
        {
            if (Vector3.Distance(barrel.transform.position, position) < range)
            {
                return barrel;
            }
        }

        return null;
    }

}

public class SoldierAI : MonoBehaviour
{
    // Simple state machine
    // - Wandering: Moving randomly
    // - Chasing: Following enemy, getting in range
    // - Shooting: Shooting at the enemy, every X seconds. If there is a barrel nearby, shoot the barrel instead
    StateMachine stateMachine;
    
    public NavMeshAgent agent;
    public Vector3 initialPos;

    public Faction faction;

    public float DetectionRange = 15.0f;
    public float ShootingRange = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        FbFManager.RegisterRecordingOption("AI");

        agent = gameObject.GetComponent<NavMeshAgent>();
        initialPos = gameObject.transform.position;

        stateMachine = new StateMachine(new Wandering(), this);
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();

        if (!GetComponent<Stats>().IsAlive())
        {
            Destroy(this);
            GetComponent<Animator>().enabled = false;
            GetComponent<NavMeshAgent>().enabled = false;

            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody childRigidbody in rigidbodies)
            {
                childRigidbody.isKinematic = false;
            }
        }
    }
}
