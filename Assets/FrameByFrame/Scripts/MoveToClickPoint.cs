using UnityEngine;
using UnityEngine.AI;
using FbF;

public class MoveToClickPoint : MonoBehaviour
{
    NavMeshAgent agent;
    void Start()
    {
        FbFManager.RegisterRecordingOption("Navigation");

        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if (FbFManager.IsRecordingOptionEnabled("Navigation"))
                {
                    PropertyGroup eventPath = FbFManager.RecordEvent(this.gameObject, "Destination changed", "Navigation");
                    eventPath.AddSphere("Destination", hit.point, 0.2f, Color.yellow, "Pathfinding");
                    eventPath.AddComment("Destination was set to something");
                    eventPath.AddLine("Marker", hit.point, hit.point + Vector3.up * 5, Color.yellow, "Pathfinding");
                }

                agent.destination = hit.point;
            }
        }
    }
}